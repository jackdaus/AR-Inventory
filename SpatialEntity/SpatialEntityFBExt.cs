using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using StereoKit;
using StereoKit.Framework;

using XrSpace            = System.UInt64;
using XrAsyncRequestIdFB = System.UInt64;

namespace SpatialEntity
{
	public class SpatialEntityFBExt : IStepper
	{
		bool extAvailable;
		bool enabled;

		public Dictionary<Guid, Anchor> Anchors = new Dictionary<Guid, Anchor>();

		public class Anchor
		{
			public XrAsyncRequestIdFB RequestId; // TODO preobably remove this>?
			public Guid Uuid; // TODO probably remove this?
			public XrSpace XrSpace;
			public Pose Pose;
			public bool IsLoaded; // TODO probably remove this?
			public bool LoadFailed;
			public bool LocateSuccess;
		}

		public bool Available => extAvailable;
		public bool Enabled { get => extAvailable && enabled; set => enabled = value; }

		public SpatialEntityFBExt() : this(true) { }
		public SpatialEntityFBExt(bool enabled = true)
		{
			if (SK.IsInitialized)
				Log.Err("SpatialEntityFBExt must be constructed before StereoKit is initialized!");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity_storage");
			Backend.OpenXR.RequestExt("XR_FB_spatial_entity_query");
		}

		public bool Initialize()
		{
			extAvailable =
				Backend.XRType == BackendXRType.OpenXR
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity")
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity_storage")
				&& Backend.OpenXR.ExtEnabled("XR_FB_spatial_entity_query")
				&& LoadBindings();

			// Set up xrPollEvent subscription
			if (extAvailable)
			{
				Backend.OpenXR.OnPollEvent += pollEventHandler;
			}

			return true;
		}

		public void Step()
		{
			// Update the anchor pose periodically because the anchor may drift slightly in the Meta Quest runtime.
			foreach(var anchor in Anchors.Values)
			{
				XrSpaceLocation spaceLocation = new XrSpaceLocation { type = XrStructureType.XR_TYPE_SPACE_LOCATION };

				// TODO consider using XrFrameState.predictedDisplayTime for XrTime argument
				XrResult result = xrLocateSpace(anchor.XrSpace, Backend.OpenXR.Space, Backend.OpenXR.Time, out spaceLocation);
				if (result == XrResult.Success)
				{
					var orientationValid = spaceLocation.locationFlags.HasFlag(XrSpaceLocationFlags.XR_SPACE_LOCATION_ORIENTATION_VALID_BIT);
					var poseValid        = spaceLocation.locationFlags.HasFlag(XrSpaceLocationFlags.XR_SPACE_LOCATION_POSITION_VALID_BIT);
					if (orientationValid && poseValid)
					{
						anchor.Pose = spaceLocation.pose;
						anchor.LocateSuccess = true;
					}
				}
				else
				{
					Log.Err($"xrLocateSpace error! Result: {result}");
					anchor.LocateSuccess = false;
				}
			}
		}

		public void Shutdown()
		{

		}

		/// <summary>
		/// Initiates the creation of a spatial anchor in the Quest system.
		/// Returns an Anchor that will be asynchronouly loaded. Check Anchor.IsLoaded to 
		/// see if the anchor has been loaded.
		/// Returns null if there was an error initiating the request to creat a new anchor.
		/// </summary>
		/// <param name="pose"></param>
		/// <returns></returns>
		public Anchor CreateAnchor(Pose pose)
		{
			if(!Enabled)
			{
				Log.Err("Spatial entity extension must be enabled before calling CreateAnchor!");
				return null;
			}

			Log.Info("Begin CreateAnchor");

			XrSpatialAnchorCreateInfoFB anchorCreateInfo = new XrSpatialAnchorCreateInfoFB(Backend.OpenXR.Space, pose, Backend.OpenXR.Time);

			XrResult result = xrCreateSpatialAnchorFB(
				Backend.OpenXR.Session,
				anchorCreateInfo,
				out XrAsyncRequestIdFB requestId);

			if (result != XrResult.Success)
			{
				Log.Err($"Error requesting creation of spatial anchor: {result}");
				return null;
			}
			
			Log.Info($"xrCreateSpatialAnchorFB initiated. The request id is: {requestId}. Result: {result}");

			// TODO 

			//var newAnchor = new Anchor()
			//{
			//	RequestId = requestId,
			//	Pose = Pose.Identity,
			//	IsLoaded = false
			//};
			//Anchors.Add(newAnchor);

			//return newAnchor;
			return null;
		}

		public void LoadAnchors()
		{
			XrSpaceQueryInfoFB queryInfo = new XrSpaceQueryInfoFB(dummy: false);
			XrResult result = xrQuerySpacesFB(Backend.OpenXR.Session, queryInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				Log.Err($"Error querying anchors! Result: {result}");
		}

		public void EraseAnchors()
		{
			foreach(var anchor in Anchors.Values)
			{
				// Initiate the async erase operation. Then we will remove it from the Dictionary once the event 
				// completes (in the poll handler).
				eraseSpace(anchor.XrSpace);
			}
		}

		// XR_FB_spatial_entity
		del_xrCreateSpatialAnchorFB               xrCreateSpatialAnchorFB;
		del_xrGetSpaceUuidFB                      xrGetSpaceUuidFB;
		del_xrEnumerateSpaceSupportedComponentsFB xrEnumerateSpaceSupportedComponentsFB;
		del_xrSetSpaceComponentStatusFB           xrSetSpaceComponentStatusFB;
		del_xrGetSpaceComponentStatusFB           xrGetSpaceComponentStatusFB;

		// XR_FB_spatial_entity_storage
		del_xrSaveSpaceFB  xrSaveSpaceFB;
		del_xrEraseSpaceFB xrEraseSpaceFB;

		// XR_FB_spatial_entity_query
		del_xrQuerySpacesFB xrQuerySpacesFB;
		del_xrRetrieveSpaceQueryResultsFB xrRetrieveSpaceQueryResultsFB;

		// Misc
		del_xrLocateSpace xrLocateSpace;

		bool LoadBindings()
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

		void pollEventHandler(IntPtr XrEventDataBufferData)
		{
			XrEventDataBuffer myBuffer = Marshal.PtrToStructure<XrEventDataBuffer>(XrEventDataBufferData);
			Log.Info($"xrPollEvent: received {myBuffer.type}");

			switch (myBuffer.type)
			{
				case XrStructureType.XR_TYPE_EVENT_DATA_SPATIAL_ANCHOR_CREATE_COMPLETE_FB:
					XrEventDataSpatialAnchorCreateCompleteFB spatialAnchorComplete = Marshal.PtrToStructure<XrEventDataSpatialAnchorCreateCompleteFB>(XrEventDataBufferData);

					if (spatialAnchorComplete.result != XrResult.Success)
					{
						Log.Err($"XrEventDataSpatialAnchorCreateCompleteFB error! Result: {spatialAnchorComplete.result}");
						break;
					}

					var newCreatedAnchor = new Anchor
					{
						RequestId = spatialAnchorComplete.requestId,
						XrSpace = spatialAnchorComplete.space,
						Uuid = spatialAnchorComplete.uuid,
						IsLoaded = true
					};
					Anchors.Add(spatialAnchorComplete.uuid, newCreatedAnchor);

					// When anchor is first created, the component STORABLE is not yet set. So we do it here.
					if (isComponentSupported(spatialAnchorComplete.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB))
					{
						XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB, true);
						XrResult setComponentResult = xrSetSpaceComponentStatusFB(spatialAnchorComplete.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

						if (setComponentResult == XrResult.XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB)
						{
							Log.Err("XR_ERROR_SPACE_COMPONENT_STATUS_ALREADY_SET_FB");

							// Save the space to storage
							saveSpace(spatialAnchorComplete.space);
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
							saveSpace(setStatusComplete.space);
						}
						else if (setStatusComplete.componentType == XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB)
						{
							// Spatial entity component was loaded from storage and component succesfully set to LOCATABLE.
							var newLoadedAnchor = new Anchor
							{
								XrSpace = setStatusComplete.space,
								Uuid = setStatusComplete.uuid,
								IsLoaded = true
							};

							Anchors.Add(setStatusComplete.uuid, newLoadedAnchor);
						}
					}
					else
					{
						Log.Err("Error from XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB: " + setStatusComplete.result);
					}

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_SAVE_COMPLETE_FB:
					XrEventDataSpaceSaveCompleteFB saveComplete = Marshal.PtrToStructure<XrEventDataSpaceSaveCompleteFB>(XrEventDataBufferData);
					if (saveComplete.result == XrResult.Success)
					{
						Log.Info("Save space sucessful!");
					}
					else
					{
						Log.Err($"Save space failed. Result: {saveComplete.result}");
						Log.Err($"XrEventDataSpaceSaveCompleteFB error! Result: {saveComplete.result}");
					}

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_ERASE_COMPLETE_FB:
					XrEventDataSpaceEraseCompleteFB eraseComplete = Marshal.PtrToStructure<XrEventDataSpaceEraseCompleteFB>(XrEventDataBufferData);
					Anchors.Remove(eraseComplete.uuid);

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_QUERY_RESULTS_AVAILABLE_FB:
					XrEventDataSpaceQueryResultsAvailableFB resultsAvailable = Marshal.PtrToStructure<XrEventDataSpaceQueryResultsAvailableFB>(XrEventDataBufferData);

					// Two call idiom to get memory space requirements
					XrSpaceQueryResultsFB queryResults = new XrSpaceQueryResultsFB(dummy: false);

					XrResult result = xrRetrieveSpaceQueryResultsFB(Backend.OpenXR.Session, resultsAvailable.requestId, out queryResults);
					if (result != XrResult.Success)
					{
						Log.Err($"xrRetrieveSpaceQueryResultsFB error! Result: {result}");
						break;
					}

					queryResults.resultCapacityInput = queryResults.resultCountOutput;

					int structByteSize = Marshal.SizeOf<XrSpaceQueryResultFB>();
					IntPtr ptr = Marshal.AllocHGlobal(structByteSize * (int)queryResults.resultCountOutput);
					queryResults.results = ptr;
					result = xrRetrieveSpaceQueryResultsFB(Backend.OpenXR.Session, resultsAvailable.requestId, out queryResults);

					if (result != XrResult.Success)
					{
						Log.Err($"xrRetrieveSpaceQueryResultsFB error! Result: {result}");
						break;
					}

					Log.Info($"Spatial entity loaded from storage! Count: {queryResults.resultCountOutput}");

					// Copy the results into managed memory (since it's easier to work with)
					List<XrSpaceQueryResultFB> resultsList = new List<XrSpaceQueryResultFB>();
					for (int i = 0; i < queryResults.resultCountOutput; i++)
					{
						XrSpaceQueryResultFB res = Marshal.PtrToStructure<XrSpaceQueryResultFB>(queryResults.results + (i * structByteSize));
						resultsList.Add(res);
					}	

					Marshal.FreeHGlobal(ptr);

					resultsList.ForEach(res =>
					{
						// Set component LOCATABLE
						if (isComponentSupported(res.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB))
						{
							XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_LOCATABLE_FB, true);
							XrResult setComponentResult = xrSetSpaceComponentStatusFB(res.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

							if (setComponentResult != XrResult.Success)
								Log.Err($"xrSetSpaceComponentStatusFB error! Result: {setComponentResult}");

							// Once this async event XR_TYPE_EVENT_DATA_SPACE_SET_STATUS_COMPLETE_FB is posted (above), this spatial entity will
							// be added to the application's Anchor list!
						}

						// Set component SORTABLE
						if (isComponentSupported(res.space, XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB))
						{
							XrSpaceComponentStatusSetInfoFB setComponentInfo = new XrSpaceComponentStatusSetInfoFB(XrSpaceComponentTypeFB.XR_SPACE_COMPONENT_TYPE_STORABLE_FB, true);
							XrResult setComponentResult = xrSetSpaceComponentStatusFB(res.space, setComponentInfo, out XrAsyncRequestIdFB requestId);

							if (setComponentResult != XrResult.Success)
								Log.Err($"xrSetSpaceComponentStatusFB error! Result: {setComponentResult}");
						}
					});

					break;
				case XrStructureType.XR_TYPE_EVENT_DATA_SPACE_QUERY_COMPLETE_FB:
					break;
				default:
					break;
			}
		}

		bool isComponentSupported(XrSpace space, XrSpaceComponentTypeFB type)
		{
			// Two call idiom to get memory space requirements
			uint numComponents = 0;
			XrResult result = xrEnumerateSpaceSupportedComponentsFB(space, 0, out numComponents, null);

			XrSpaceComponentTypeFB[] componentTypes = new XrSpaceComponentTypeFB[numComponents];
			result = xrEnumerateSpaceSupportedComponentsFB(space, numComponents, out numComponents, componentTypes);

			if (result != XrResult.Success)
			{
				Log.Err($"xrEnumerateSpaceSupportedComponentsFB Error! Result: {result}");
			}

			for (int i = 0; i < numComponents; i++)
			{
				if (componentTypes[i] == type)
					return true;
			}

			return false;
		}

		void saveSpace(XrSpace space)
		{
			XrSpaceSaveInfoFB saveInfo = new XrSpaceSaveInfoFB(space, XrSpaceStorageLocationFB.XR_SPACE_STORAGE_LOCATION_LOCAL_FB, XrSpacePersistenceModeFB.XR_SPACE_PERSISTENCE_MODE_INDEFINITE_FB);
			XrResult result = xrSaveSpaceFB(Backend.OpenXR.Session, saveInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				Log.Err($"xrSaveSpaceFB! Result: {result}");
		}

		void eraseSpace(XrSpace space)
		{
			XrSpaceEraseInfoFB eraseInfo = new XrSpaceEraseInfoFB(space, XrSpaceStorageLocationFB.XR_SPACE_STORAGE_LOCATION_LOCAL_FB);
			XrResult result = xrEraseSpaceFB(Backend.OpenXR.Session, eraseInfo, out XrAsyncRequestIdFB requestId);
			if (result != XrResult.Success)
				Log.Err($"xrEraseSpaceFB! Result: {result}");
		}
	}
}