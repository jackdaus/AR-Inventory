using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static StereoKitFBSpatialEntity.SpatialEntityFBExt;

namespace AR_Inventory.Steppers
{
    /// <summary>
    /// Manages the UI for edditing the inventory items.
    /// Also responsible for rendering the items.
    /// </summary>
    internal class ManageInventory : IStepper
    {
        public bool Enabled { get; set; }

        Pose _pose = new Pose(0.2f, 0, -0.4f, Quat.LookDir(0, 0, 1));

        Model _model;
        Model _shellModel;

        Material _shellMaterial;

        Pose _editWindowPose = new Pose(0.2f, 0, -0.4f, Quat.LookDir(0, 0, 1));
        Vec2 _inputSize = new Vec2(15 * U.cm, 3 * U.cm);

        TextStyle _largeTextStyle;

        public bool Initialize()
        {
            _largeTextStyle = Text.MakeStyle(Font.Default, 0.02f, Color.White);

            // The shell material has transparency. So we set that up here.
            _shellMaterial = Material.Default.Copy();
            _shellMaterial.Transparency = Transparency.Blend;
            _shellMaterial.DepthWrite = false;

            _model = Model.FromMesh(Mesh.GenerateSphere(0.05f), Default.MaterialUI);
            _shellModel = Model.FromMesh(Mesh.GenerateSphere(0.2f), _shellMaterial);

            return true;
        }

        public void Shutdown()
        {

        }

        public void Step()
        {
            // Store the hands for the frame since we will use them in a few places
            Hand lHand = Input.Hand(Handed.Left);
            Hand rHand = Input.Hand(Handed.Right);

            UI.WindowBegin("manage-inventory-window", ref _pose, UIWin.Body);

            if (App.Passthrough.Available)
            {
                if (UI.ButtonRound("Passthrough", Catalog.Sprites.IconEye))
                    App.Passthrough.Enabled = !App.Passthrough.Enabled;
            }

            if (UI.Button("New item"))
                CreateNewItem();

            if (UI.Button("Quit"))
                SK.Quit();

            UI.WindowEnd();


            // Render Items
            //
            foreach (ItemDto item in App.ItemService.Items.ToList())
            {
                // Try to get spatial anchor for the item
                StereoKitFBSpatialEntity.SpatialEntityFBExt.Anchor? anchor = App.ItemService.TryGetSpatialAnchor(item);

                // If spatial anchors are present and loaded, we will use the anchor as a root for the item.
                // Otherwise, we will "gracefully" fall back to just the item's local pose relative to the
                // anchor root (mostly for dev purposes)
                if (anchor != null)
                    Hierarchy.Push(anchor.Value.Pose.ToMatrix());

                // Update location data in persistent storage
                if (UI.Handle(item.Id.ToString(), ref item.Pose, _model.Bounds))
                    Controller.UpdateItem(item); // TODO this could be made more efficient by only updating once the UI handle is released. Or maybe setting a throttle (if network connected, for real-time collab)

                // Highlight the item if it's the search result. Shell color is a little transparent.
                Color modelColor = App.ItemService.SearchedItem == item ? new Color(0, 1, 1) : Color.White;

                // Shell color is same as model color, but alpha = 0.1
                Color shellColor = modelColor;
                shellColor.a = 0.2f;

                // Draw the items!
                _model.Draw(item.Pose.ToMatrix(), modelColor);

                // For the minimap! We draw the model again as well as the shell. Making sure to specify
                // RenderLayer.ThirdPerson, since this is what the minimap is going off of.
                float size = App.ItemService.SearchedItem == item ? 2 : 1;
                _model.Draw(item.Pose.ToMatrix(size), modelColor, layer: RenderLayer.ThirdPerson);
                _shellModel.Draw(item.Pose.ToMatrix(size), shellColor, layer: RenderLayer.ThirdPerson);

                // Item label floats 10cm above the object
                Vec3 titlePosition = item.Pose.position;
                titlePosition.y += 12 * U.cm;

                Quat titleOrientation = Quat.LookAt(Hierarchy.ToWorld(item.Pose.position), Input.Head.position);

                // Correct the text angle to remove rotation from the anchor
                if (anchor != null)
                {
                    Quat inverseRootOrientation = anchor.Value.Pose.orientation.Inverse;
                    titleOrientation = inverseRootOrientation * titleOrientation;
                }

                if (App.ItemService.FocusedItem?.Id == item.Id)
                {
                    // TODO set UIBox scale to adjust to smaller/larger models
                    Mesh.Cube.Draw(Material.UIBox, item.Pose.ToMatrix(0.12f));

                    titlePosition.y += 0 * U.cm;
                    _editWindowPose.position = titlePosition;
                    _editWindowPose.orientation = titleOrientation;

                    UI.WindowBegin("edit-title-window", ref _editWindowPose, UIWin.Body);
                    if (UI.Input("title-input", ref item.Title, _inputSize))
                    {
                        Controller.UpdateItem(item);
                    }
                    UI.SameLine();
                    if (UI.ButtonRound("clear-item-name-button", Catalog.Sprites.IconClear))
                    {
                        item.Title = string.Empty;
                        Controller.UpdateItem(item);
                    }
                    UI.SameLine();
                    if (UI.ButtonRound("delete-item-button", Catalog.Sprites.IconDelete))
                    {
                        Controller.DeleteItem(item.Id);
                        App.ItemService.Items.Remove(item);
                        if (App.ItemService.SearchedItem == item)
                        {
                            App.ItemService.SearchedItem = null;
                        }
                    }
                    if (App.DEBUG_ON)
                    {
                        UI.Label($"SpatialAnchorUuid: {item.SpatialAnchorUuid}");
                    }
                    UI.WindowEnd();
                }
                else
                {
                    Pose titlePose = new Pose(titlePosition, titleOrientation);
                    UI.EnableFarInteract = false;
                    UI.WindowBegin($"title-{item.Id}", ref titlePose, UIWin.Body);
                    UI.PushTextStyle(_largeTextStyle);
                    UI.Label(item.Title);
                    UI.PopTextStyle();

                    // Display warning icon if anchor cannot be found
                    if (anchor == null)
                        Catalog.Sprites.IconWarning.Draw(Matrix.TS(Vec3.Forward * 0.001f, 0.05f), TextAlign.BottomCenter, new Color(1, 0.8f, 0));

                    UI.WindowEnd();
                    UI.EnableFarInteract = true;
                }

                if (anchor != null)
                    Hierarchy.Pop();
            }


            // Update focused item
            //
            if (App.ItemService.FocusedItem == null)
            {
                App.ItemService.FocusedItem = FirstItemTouchedByFinger(App.ItemService.Items, lHand, rHand);

                // Play a sound if an item was just selected
                if (App.ItemService.FocusedItem != null)
                    Sound.Click.Play(App.ItemService.FocusedItem.Pose.position); // TODO adjust to anchor space
            }
            else
            {
                var center = App.ItemService.FocusedItem.Pose.position;

                // Try to get spatial anchor for the item
                StereoKitFBSpatialEntity.SpatialEntityFBExt.Anchor? anchor = App.ItemService.TryGetSpatialAnchor(App.ItemService.FocusedItem);

                // Adjust to anchor space if available
                if (anchor != null)
                    center = anchor.Value.Pose.ToMatrix() * center;

                Bounds focusedItemBounds = new Bounds(center, _model.Bounds.dimensions);

                // Draw intersection bounds in red
                if (App.DEBUG_ON)
                {
                    //_model.Draw(Matrix.T(focusedItemBounds.center), new Color(1, 0, 0));
                }

                bool stillTouchingFocusedItem = AnyFingerTipTouching(focusedItemBounds, lHand, rHand);

                if (!stillTouchingFocusedItem)
                {
                    var touchedItem = FirstItemTouchedByFinger(App.ItemService.Items, lHand, rHand);
                    if (touchedItem != null && touchedItem != App.ItemService.FocusedItem)
                    {
                        // Touching a different item this frame. Make it the new focused item and make a sound.
                        App.ItemService.FocusedItem = touchedItem;
                        Sound.Click.Play(center);
                    }
                }
            }
        }

        public void AddItem()
        {
            CreateNewItem();
        }

        private ItemDto FirstItemTouchedByFinger(List<ItemDto> items, Hand leftHand, Hand rightHand)
        {
            foreach (var item in items)
            {
                var center = item.Pose.position;

                // Try to get spatial anchor for the item
                StereoKitFBSpatialEntity.SpatialEntityFBExt.Anchor? anchor = App.ItemService.TryGetSpatialAnchor(item);

                // Adjust to anchor space if available
                if (anchor != null)
                    center = anchor.Value.Pose.ToMatrix() * center;

                Bounds itemBounds = new Bounds(center, _model.Bounds.dimensions);

                // Draw intersection bounds in green
                if (App.DEBUG_ON)
                {
                    //_model.Draw(Matrix.T(itemBounds.center), new Color(0, 1, 0));
                }

                if (AnyFingerTipTouching(itemBounds, leftHand, rightHand))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if any finger tip is in the bounds. We pass in the Hand data so we don't make multiple calls to Input.Hand
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="rightHand"></param>
        /// <param name="leftHand"></param>
        /// <returns></returns>
        private bool AnyFingerTipTouching(Bounds bounds, Hand leftHand, Hand rightHand)
        {
            // Only consider tracked hands. Untracked hands can still have a position, even if not visible!
            if (rightHand.IsTracked)
            {
                // Loop over all 5 fingers
                for (int f = 0; f <= (int)FingerId.Little; f++)
                {
                    Pose fingertip = rightHand[(FingerId)f, JointId.Tip].Pose;
                    if (bounds.Contains(fingertip.position))
                        return true;
                }
            }

            if (leftHand.IsTracked)
            {
                // Loop over all 5 fingers
                for (int f = 0; f <= (int)FingerId.Little; f++)
                {
                    Pose fingertip = leftHand[(FingerId)f, JointId.Tip].Pose;
                    if (bounds.Contains(fingertip.position))
                        return true;
                }
            }

            return false;
        }

        // Give a unique index to new items, for readability 
        private int _newItemCounter = 1;

        private void CreateNewItem()
        {
            // This new item will appear directly in front of user's head
            var newItemPosition = Input.Head.position + Input.Head.Forward * 0.5f;
            var newItemPose = new Pose(newItemPosition, Quat.Identity);

            var newItemDto = new ItemDto
            {
                Id = Guid.NewGuid(),
                Pose = Pose.Identity,
                Title = $"NEW ITEM #{_newItemCounter}",
            };

            _newItemCounter++;

            // The anchor id will be asynchronously set via a callback once it's finished being created
            App.SpatialEntity.CreateAnchor(newItemPose,
                (newId) =>
                    {
                        newItemDto.SpatialAnchorUuid = newId;
                        // Save the item so the id persists in our db storage
                        // NOTE: would be better if UpdateItem just set the anchor id... 
                        // to avoid overwriting the Title in case it has since changed
                        Controller.UpdateItem(newItemDto);
                    },
                () => newItemDto.SpatialAnchorCreateFailed = true);

            Controller.AddItem(newItemDto);
            App.ItemService.Items.Add(newItemDto);
            App.ItemService.FocusedItem = newItemDto;
        }
    }
}
