// This requires an addition to the Android Manifest to work on quest:
// <uses-permission android:name="com.oculus.permission.USE_ANCHOR_API"/>
//
// To work on Quest+Link, you may need to enable beta features in the Oculus
// app's settings.

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using StereoKit;
using StereoKit.Framework;
using System.Linq;

using XrSpace            = System.UInt64;
using XrAsyncRequestIdFB = System.UInt64;

namespace StereoKitFBSpatialEntity
{
    public class SpatialEntityFBExt : IStepper
	{
        private bool _extAvailable;
		private bool _enabled;
		private Dictionary<Guid, Anchor> _anchors = new Dictionary<Guid, Anchor>();
		private Dictionary<XrAsyncRequestIdFB, CreateCallback> _createCallbacks = new Dictionary<XrAsyncRequestIdFB, CreateCallback>();
		private bool _loadAnchorsOnInit;

        public bool Available => _extAvailable;
		public bool Enabled { get => _extAvailable && _enabled; set => _enabled = value; }
        public int AnchorCount => _anchors.Count;
        public IReadOnlyCollection<Anchor> Anchors => _anchors.Values;

		/// <summary>
		/// Set a log level filter for this extension so that we can easily print info when debugging, without cluttering up the output
		/// when not needed.
		/// </summary>
		public LogLevel LogLevelFilter = LogLevel.Info;

        public SpatialEntityFBExt() : this(true) { }
        public SpatialEntityFBExt(bool loadAnchorsOnInit = false)
		{
			if (SK.IsInitialized)
				LogSpatialEntityFBExt(LogLevel.Error, "SpatialEntityFBExt must be constructed before StereoKit is initialized!");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity_storage");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity_query");

            _loadAnchorsOnInit = loadAnchorsOnInit;
		}

		public bool Initialize()
		{
			_extAvailable =
				Backend.XRType == BackendXRType.OpenXR
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity")
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity_storage")
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity_query")
				&& LoadBindings();

			// Set up xrPollEvent subscription
			if (_extAvailable)
			{
				Backend.OpenXR.OnPollEvent += PollEventHandler;
			}

			// Enable on initialization
			_enabled = true;

			// Load all anchors from storage.
			if (_loadAnchorsOnInit)
				LoadAllAnchors();

			return true;
		}

		public void Step()
		{
            // Update the anchor pose periodically because the anchor may drift slightly in the Meta Quest runtime.
            foreach (var entry in _anchors.ToArray())
            {
                var anchor = entry.Value;

                XrSpaceLocation spaceLocation = new XrSpaceLocation { type = XrStructureType.XR_TYPE_SPACE_LOCATION };

                // TODO consider using XrFrameState.predictedDisplayTime for XrTime argument
                XrResult result = xrLocateSpace(anchor.XrSpace, Backend.OpenXR.Space, Backend.OpenXR.Time, out spaceLocation);
                if (result == XrResult.Success)
                {
                    var orientationValid = spaceLocation.locationFlags.HasFlag(XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT);
                    var poseValid = spaceLocation.locationFlags.HasFlag(XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT);
                    if (orientationValid && poseValid)
                    {
                        anchor.Pose = spaceLocation.pose;
                        anchor.LocateSuccess = true;
                    }

                    // Update the struct anchor value
                    _anchors[anchor.Uuid] = anchor;
                }
                else
                {
					LogSpatialEntityFBExt(LogLevel.Error, $"xrLocateSpace error! Result: {result}");
                    anchor.LocateSuccess = false;
                }
            }
        }

		public void Shutdown()
		{

		}

		/// <summary>
		/// Try to get an Anchor based on the Uuid.
		/// </summary>
		/// <param name="SpatialAnchorUuid"></param>
		/// <returns></returns>
        public Anchor? TryGetSpatialAnchor(Guid SpatialAnchorUuid)
        {
            // We need to check if the TryGetValue was successful. If not successful, the anchor will be set to "empty" anchor
			// struct. But we want to return a null value in that case!
            if (_anchors.TryGetValue(SpatialAnchorUuid, out var anchor))
				return anchor;
			
			return null;
		}

		/// <summary>
		/// Load all avaiable Anchors into the runtime.
		/// </summary>
        public void LoadAllAnchors()
        {
            XrSpaceQueryInfoFB queryInfo = new XrSpaceQueryInfoFB();
            XrResult result = xrQuerySpacesFB(Backend.OpenXR.Session, queryInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				LogSpatialEntityFBExt(LogLevel.Error, $"Error querying anchors! Result: {result}");
        }

        /// <summary>
        /// Create a spatial anchor at the given pose! This is an asynchronous action. You can optionally provide
        /// some callbacks for when the action completes (either success or fail). 
        /// </summary>
        /// <param name="pose">The pose where the new anchor should be created.</param>
        /// <param name="onSuccessCallback">The action to be performed when the anchor is successfully created. This is a delegate that has one Guid parameter (the new anchor id).</param>
        /// <param name="onErrorCallback">The action to perform when the create anchor fails. This is a delegate with no parameters.</param>
        /// <returns></returns>
		public bool CreateAnchor(Pose pose, OnCreateSuccessDel onSuccessCallback = null, OnCreateErrorDel onErrorCallback = null)
		{
			LogSpatialEntityFBExt(LogLevel.Info, "Begin CreateAnchor");

			if (!Enabled)
			{
				LogSpatialEntityFBExt(LogLevel.Error, "Spatial entity extension must be enabled before calling CreateAnchor!");
                return false;
			}

			XrSpatialAnchorCreateInfoFB anchorCreateInfo = new XrSpatialAnchorCreateInfoFB(Backend.OpenXR.Space, pose, Backend.OpenXR.Time);

			XrResult result = xrCreateSpatialAnchorFB(
				Backend.OpenXR.Session,
				anchorCreateInfo,
				out XrAsyncRequestIdFB requestId);

			if (result != XrResult.Success)
			{
				LogSpatialEntityFBExt(LogLevel.Error, $"Error requesting creation of spatial anchor: {result}");
				return false;
            }

			LogSpatialEntityFBExt(LogLevel.Info, $"xrCreateSpatialAnchorFB initiated. The request id is: {requestId}.");

            var createCallback = new CreateCallback
			{
				OnSuccess = onSuccessCallback,
				OnError = onErrorCallback
			};

			_createCallbacks.Add(requestId, createCallback);

			return true;
		}

		/// <summary>
		/// Delete an Anchor from the system. Note: the Anchor must currently be loaded in order to delete it.
		/// </summary>
		/// <param name="uuid">The Uuid of the anchor to delete.</param>
		/// <returns></returns>
		public void DeleteAnchor(Guid AnchorUuid)
		{
			if (_anchors.TryGetValue(AnchorUuid, out Anchor anchor))
			{
                // Initiate the async erase operation. Anchor won't be removed until the async operation completes.
                EraseSpace(anchor.XrSpace);
            }
		}

		/// <summary>
		/// Delete all loaded Anchors from the system. Note: this will only delete the anchors currently LOADED into the runtime.
		/// It will not delete anchors that are not loaded into the runtime.
		/// </summary>
		public void DeleteAllAnchors()
		{
			foreach (var anchor in _anchors.Values)
			{
                // Initiate the async erase operation. Anchor won't be removed until the async operation completes.
				EraseSpace(anchor.XrSpace);
            }
		}
        
		private void PollEventHandler(IntPtr XrEventDataBufferData)
		{
			XrEventDataBuffer myBuffer = Marshal.PtrToStructure<XrEventDataBuffer>(XrEventDataBufferData);
			LogSpatialEntityFBExt(LogLevel.Info, $"xrPollEvent: received {myBuffer.type}");

			switch (myBuffer.type)
			{
				case XrStructureType.XR_TYPE_EVENT_DATA_SPATIAL_ANCHOR_CREATE_COMPLETE_FB:
					XrEventDataSpatialAnchorCreateCompleteFB spatialAnchorComplete = Marshal.PtrToStructure<XrEventDataSpatialAnchorCreateCompleteFB>(XrEventDataBufferData);

					if (!_createCallbacks.TryGetValue(spatialAnchorComplete.requestId, out CreateCallback callack))
					{
						LogSpatialEntityFBExt(LogLevel.Error, $"Unable to find callback for the anchor created request with Id: {spatialAnchorComplete.requestId}!");
						break;
					}

					if (spatialAnchorComplete.result != XrResult.Success)
					{
						LogSpatialEntityFBExt(LogLevel.Error, $"XrEventDataSpatialAnchorCreateCompleteFB error! Result: {spatialAnchorComplete.result}");
						if (callack.OnError != null)
							callack.OnError();
						break;
					}

					Anchor newCreatedAnchor = new Anchor
					{
						Uuid = spatialAnchorComplete.uuid,
						XrSpace = spatialAnchorComplete.space,
                    };

					_anchors.Add(spatialAnchorComplete.uuid, newCreatedAnchor);

					if (callack.OnSuccess != null)
						callack.OnSuccess(spatialAnchorComplete.uuid);

					// When anchor is first created, the component STORABLE is not yet set. So we do it here.
					if (IsComponentSupported(spatialAnchorComplete.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB))
					{
						XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB, true);
						XrResult setComponentResult = xrSetSpaceComponentStatusFB(spatialAnchorComplete.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

						if (setComponentResult == XrResult.XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB)
						{
							LogSpatialEntityFBExt(LogLevel.Error, "XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB");
							
							// Save the space to storage
							SaveSpace(spatialAnchorComplete.space);
						}
					}

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB:
					XrEventDataSpaceSetStatusCompleteFB setStatusComplete = Marshal.PtrToStructure<XrEventDataSpaceSetStatusCompleteFB>(XrEventDataBufferData);
					if (setStatusComplete.result == XrResult.Success)
					{
						if (setStatusComplete.componentType == XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB)
						{
							// Save space
							SaveSpace(setStatusComplete.space);
						}
						else if (setStatusComplete.componentType == XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB)
						{
							// Spatial entity component was loaded from storage and component succesfully set to LOCATABLE.
							var newLoadedAnchor = new Anchor
							{
								Uuid = setStatusComplete.uuid,
								XrSpace = setStatusComplete.space,
                            };

							_anchors.Add(setStatusComplete.uuid, newLoadedAnchor);
						}
					}
					else
					{
						LogSpatialEntityFBExt(LogLevel.Error, "Error from XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB: " + setStatusComplete.result);
					}

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_SAVE_COMPLETE_FB:
					XrEventDataSpaceSaveCompleteFB saveComplete = Marshal.PtrToStructure<XrEventDataSpaceSaveCompleteFB>(XrEventDataBufferData);
					if (saveComplete.result == XrResult.Success)
					{
						LogSpatialEntityFBExt(LogLevel.Info, "Save space sucessful!");
					}
					else
					{
						LogSpatialEntityFBExt(LogLevel.Error, $"Save space failed. XrEventDataSpaceSaveCompleteFB error! Result: {saveComplete.result}");
					}

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_ERASE_COMPLETE_FB:
					XrEventDataSpaceEraseCompleteFB eraseComplete = Marshal.PtrToStructure<XrEventDataSpaceEraseCompleteFB>(XrEventDataBufferData);
					_anchors.Remove(eraseComplete.uuid);

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_QUERY_RESULTS_AVAILABLE_FB:
					XrEventDataSpaceQueryResultsAvailableFB resultsAvailable = Marshal.PtrToStructure<XrEventDataSpaceQueryResultsAvailableFB>(XrEventDataBufferData);

					// Two call idiom to get memory space requirements
					XrSpaceQueryResultsFB queryResults = new XrSpaceQueryResultsFB();

					XrResult result = xrRetrieveSpaceQueryResultsFB(Backend.OpenXR.Session, resultsAvailable.requestId, out queryResults);
					if (result != XrResult.Success)
					{
						LogSpatialEntityFBExt(LogLevel.Error, $"xrRetrieveSpaceQueryResultsFB error! Result: {result}");
						break;
					}

					queryResults.resultCapacityInput = queryResults.resultCountOutput;

					int structByteSize = Marshal.SizeOf<XrSpaceQueryResultFB>();
					IntPtr ptr = Marshal.AllocHGlobal(structByteSize * (int)queryResults.resultCountOutput);
					List<XrSpaceQueryResultFB> resultsList = new List<XrSpaceQueryResultFB>();

					try
					{
						queryResults.results = ptr;
						result = xrRetrieveSpaceQueryResultsFB(Backend.OpenXR.Session, resultsAvailable.requestId, out queryResults);

						if (result != XrResult.Success)
						{
							LogSpatialEntityFBExt(LogLevel.Error, $"xrRetrieveSpaceQueryResultsFB error! Result: {result}");
							break;
						}

						LogSpatialEntityFBExt(LogLevel.Info, $"Spatial entities loaded from storage! Count: {queryResults.resultCountOutput}");

						// Copy the results into managed memory (since it's easier to work with)
						for (int i = 0; i < queryResults.resultCountOutput; i++)
						{
							XrSpaceQueryResultFB res = Marshal.PtrToStructure<XrSpaceQueryResultFB>(queryResults.results + (i * structByteSize));
							resultsList.Add(res);
						}	
					}
					finally
					{
						// Make sure we always free the allocated memory to prevent a memory leak!
						Marshal.FreeHGlobal(ptr);
					}

					resultsList.ForEach(res =>
					{
						// Set component LOCATABLE
						if (IsComponentSupported(res.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB))
						{
							XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB, true);
							XrResult setComponentResult = xrSetSpaceComponentStatusFB(res.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

							if (setComponentResult != XrResult.Success  && setComponentResult != XrResult.XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB)
								LogSpatialEntityFBExt(LogLevel.Error, $"xrSetSpaceComponentStatusFB error! Result: {setComponentResult}");

							// Once this async event XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB is posted (above), this spatial entity will
							// be added to the application's Anchor list!
						}

						// Set component STORABLE
						if (IsComponentSupported(res.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB))
						{
							XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB, true);
							XrResult setComponentResult = xrSetSpaceComponentStatusFB(res.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

							if (setComponentResult != XrResult.Success && setComponentResult != XrResult.XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB)
								LogSpatialEntityFBExt(LogLevel.Error, $"xrSetSpaceComponentStatusFB error! Result: {setComponentResult}");
						}
					});

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_QUERY_COMPLETE_FB:
					break;
				default:
					break;
			}
		}

		private bool IsComponentSupported(XrSpace space, XrSpaceComponentTypeFB type)
		{
			// Two call idiom to get memory space requirements
			uint numComponents = 0;
			XrResult result = xrEnumerateSpaceSupportedComponentsFB(space, 0, out numComponents, null);

			XrSpaceComponentTypeFB[] componentTypes = new XrSpaceComponentTypeFB[numComponents];
			result = xrEnumerateSpaceSupportedComponentsFB(space, numComponents, out numComponents, componentTypes);

			if (result != XrResult.Success)
			{
				LogSpatialEntityFBExt(LogLevel.Error, $"xrEnumerateSpaceSupportedComponentsFB Error! Result: {result}");
			}

			for (int i = 0; i < numComponents; i++)
			{
				if (componentTypes[i] == type)
					return true;
			}

			return false;
		}

		private void SaveSpace(XrSpace space)
		{
			XrSpaceSaveInfoFB saveInfo = new XrSpaceSaveInfoFB(space, XrSpaceStorageLocationFB.XR_SPACE_STORAGE_LOCATION_LOCAL_FB, XrSpacePersistenceModeFB.XR_SPACE_PERSISTENCE_MODE_INDEFINITE_FB);
			XrResult result = xrSaveSpaceFB(Backend.OpenXR.Session, saveInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				LogSpatialEntityFBExt(LogLevel.Error, $"xrSaveSpaceFB! Result: {result}");
		}

        private void EraseSpace(XrSpace space)
		{
            // Initiate the async erase operation. Then we will remove it from the Dictionary once the event completes (in the poll event handler).
            XrSpaceEraseInfoFB eraseInfo = new XrSpaceEraseInfoFB(space, XrSpaceStorageLocationFB.XR_SPACE_STORAGE_LOCATION_LOCAL_FB);
			XrResult result = xrEraseSpaceFB(Backend.OpenXR.Session, eraseInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				LogSpatialEntityFBExt(LogLevel.Error, $"xrEraseSpaceFB! Result: {result}");
		}

		private void LogSpatialEntityFBExt(LogLevel logLevel, string message)
		{
            if (logLevel >= this.LogLevelFilter)
				Log.Write(logLevel, message);
        }

        #region OpenXR function bindings
        // XR_FB_spatial_entity
        del_xrCreateSpatialAnchorFB xrCreateSpatialAnchorFB;
        del_xrGetSpaceUuidFB xrGetSpaceUuidFB;
        del_xrEnumerateSpaceSupportedComponentsFB xrEnumerateSpaceSupportedComponentsFB;
        del_xrSetSpaceComponentStatusFB xrSetSpaceComponentStatusFB;
        del_xrGetSpaceComponentStatusFB xrGetSpaceComponentStatusFB;

        // XR_FB_spatial_entity_storage
        del_xrSaveSpaceFB xrSaveSpaceFB;
        del_xrEraseSpaceFB xrEraseSpaceFB;

        // XR_FB_spatial_entity_query
        del_xrQuerySpacesFB xrQuerySpacesFB;
        del_xrRetrieveSpaceQueryResultsFB xrRetrieveSpaceQueryResultsFB;

        // Misc
        del_xrLocateSpace xrLocateSpace;

        private bool LoadBindings()
        {
            // XR_FB_spatial_entity
            xrCreateSpatialAnchorFB = Backend.OpenXR.GetFunction<del_xrCreateSpatialAnchorFB>("xrCreateSpatialAnchorFB");
            xrGetSpaceUuidFB = Backend.OpenXR.GetFunction<del_xrGetSpaceUuidFB>("xrGetSpaceUuidFB");
            xrEnumerateSpaceSupportedComponentsFB = Backend.OpenXR.GetFunction<del_xrEnumerateSpaceSupportedComponentsFB>("xrEnumerateSpaceSupportedComponentsFB");
            xrSetSpaceComponentStatusFB = Backend.OpenXR.GetFunction<del_xrSetSpaceComponentStatusFB>("xrSetSpaceComponentStatusFB");
            xrGetSpaceComponentStatusFB = Backend.OpenXR.GetFunction<del_xrGetSpaceComponentStatusFB>("xrGetSpaceComponentStatusFB");

            // XR_FB_spatial_entity_storage
            xrSaveSpaceFB = Backend.OpenXR.GetFunction<del_xrSaveSpaceFB>("xrSaveSpaceFB");
            xrEraseSpaceFB = Backend.OpenXR.GetFunction<del_xrEraseSpaceFB>("xrEraseSpaceFB");

            // XR_FB_spatial_entity_query
            xrQuerySpacesFB = Backend.OpenXR.GetFunction<del_xrQuerySpacesFB>("xrQuerySpacesFB");
            xrRetrieveSpaceQueryResultsFB = Backend.OpenXR.GetFunction<del_xrRetrieveSpaceQueryResultsFB>("xrRetrieveSpaceQueryResultsFB");

            // Misc
            xrLocateSpace = Backend.OpenXR.GetFunction<del_xrLocateSpace>("xrLocateSpace");

            return xrCreateSpatialAnchorFB != null
                && xrGetSpaceUuidFB != null
                && xrEnumerateSpaceSupportedComponentsFB != null
                && xrSetSpaceComponentStatusFB != null
                && xrGetSpaceComponentStatusFB != null
                && xrSaveSpaceFB != null
                && xrEraseSpaceFB != null
                && xrQuerySpacesFB != null
                && xrRetrieveSpaceQueryResultsFB != null
                && xrLocateSpace != null;
        }
        #endregion

        #region Nested types
        public struct Anchor
        {
            public Guid Uuid;
            public XrSpace XrSpace;
            public Pose Pose;
            public bool LocateSuccess;
        }

        public delegate void OnCreateSuccessDel(Guid newAnchorUuid);
        public delegate void OnCreateErrorDel();

        private class CreateCallback
        {
            public OnCreateSuccessDel OnSuccess;
            public OnCreateErrorDel OnError;
        }
        #endregion
    }
}