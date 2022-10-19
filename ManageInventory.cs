using ARInventory.Entities.Models;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory
{
    /// <summary>
    /// Manages the UI for edditing the inventory items.
    /// Responsible for rendering the items.
    /// </summary>
    internal class ManageInventory : IStepper
    {
        public bool Enabled { get; set; }

        Pose _pose = new Pose(0.2f, 0, -0.4f, Quat.LookDir(0, 0, 1));
        
        Model _model;

        Pose _editWindowPose = new Pose(0.2f, 0, -0.4f, Quat.LookDir(0, 0, 1));
        Vec2 _inputSize = new Vec2(15 * U.cm, 3 * U.cm);

        // En lieu of spatial anchors, we use a root anchor that can be manually calibrated
        Pose _rootAnchor = new Pose(0, 0, -0.2f);

        public bool Initialize()
        {
            _model = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI);

            SK.AddStepper(new HandMenuRadial(
                new HandRadialLayer("Root",
                    new HandMenuItem("New", null, () => createNewItem()),
                    new HandMenuItem("About", null, () => Log.Info(SK.VersionName)),
                    new HandMenuItem("Cancel", null, null))
                ));

            return true;
        }

        public void Shutdown()
        {
            
        }

        public void Step()
        {
            // Store the hands for the frame since we will reuse them
            Hand lHand = Input.Hand(Handed.Left);
            Hand rHand = Input.Hand(Handed.Right);

            UI.WindowBegin("manage-inventory-window", ref _pose, UIWin.Body);

            if (App.Passthrough.Available)
            {
                if (UI.ButtonRound("Passthrough", Catalog.Sprites.IconEye))
                    App.Passthrough.EnabledPassthrough = !App.Passthrough.EnabledPassthrough;
            }

            if (UI.Button("New item"))
            {
                createNewItem();
            }

            if (UI.Button("Load all items"))
            {
                App.ItemService.ReloadItems();
            }

            UI.WindowEnd();


            // Root Anchor
            //
            UI.Handle("root-anchor", ref _rootAnchor, _model.Bounds);
            _model.Draw(_rootAnchor.ToMatrix(), new Color(0, 0, 1));

            // Render Items
            //
            Hierarchy.Push(_rootAnchor.ToMatrix());
            ItemDto itemToRemove = null;
            foreach(ItemDto item in App.ItemService.Items)
            {
                if (UI.Handle(item.Id.ToString(), ref item.Pose, _model.Bounds))
                {
                    // TODO this could be made more efficient by only updating once the UI handle is released
                    Controller.UpdateItem(item);
                }

                // Highlight the item if it's the search result
                Color modelColor = App.ItemService.SearchedItem == item ? new Color(1,0,0) : Color.White;
                _model.Draw(item.Pose.ToMatrix(), modelColor);

                // Item label floats 10cm above the object
                Vec3 textPosition = item.Pose.position;
                textPosition.y += 10 * U.cm;

                Quat inverseRootOrientation = _rootAnchor.orientation.Inverse;
                Quat textLookAtOrientation  = Quat.LookAt(Hierarchy.ToWorld(item.Pose.position), Input.Head.position);
                Quat textOrientation = inverseRootOrientation * textLookAtOrientation;

                Matrix textTransform = Matrix.TR(textPosition, textOrientation);
                Text.Add(item.Title, textTransform, Color.Black);

                if (App.ItemService.FocusedItem == item)
                {
                    // TODO set UIBox scale to adjust to smaller/larger models
                    Mesh.Cube.Draw(Material.UIBox, item.Pose.ToMatrix(0.12f));

                    textPosition.y += 8 * U.cm;
                    _editWindowPose.position = textPosition;
                    _editWindowPose.orientation = textOrientation;

                    UI.WindowBegin("edit-title-window", ref _editWindowPose, UIWin.Body);
                    if (UI.Input( "title-input", ref item.Title, _inputSize))
                    {
                        Controller.UpdateItem(item);
                    }
                    UI.SameLine();
                    if (UI.ButtonRound("delete-item-button", Catalog.Sprites.IconDelete))
                    {
                        itemToRemove = item;
                    }
                    UI.WindowEnd();
                }
            }
            
            // We must remove the item from the list while outside of the foreach loop.
            // A collection cannot be modified while being enumerated!
            if (itemToRemove != null)
            {
                Controller.DeleteItem(itemToRemove.Id);
                App.ItemService.Items.Remove(itemToRemove);
            }

            // Update focused item
            //
            if (App.ItemService.FocusedItem == null)
            {
                App.ItemService.FocusedItem = firstItemTouchedByFinger(App.ItemService.Items, lHand, rHand);
            }
            else
            {
                Bounds focusedItemBounds = new Bounds(Hierarchy.ToWorld(App.ItemService.FocusedItem.Pose.position), _model.Bounds.dimensions);
                bool stillTouchingFocusedItem = anyFingerTipTouching(focusedItemBounds, lHand, rHand);

                if (!stillTouchingFocusedItem)
                {
                    var touchedItem = firstItemTouchedByFinger(App.ItemService.Items, lHand, rHand);
                    if (touchedItem != null && touchedItem != App.ItemService.FocusedItem)
                    {
                        // Touching a different item this frame. Make it the new focused item and make a sound
                        App.ItemService.FocusedItem = touchedItem;
                        Sound.Click.Play(Hierarchy.ToWorld(touchedItem.Pose.position));
                    }
                }
            }
            Hierarchy.Pop();

        }

        private ItemDto firstItemTouchedByFinger(List<ItemDto> items, Hand leftHand, Hand rightHand)
        {
            foreach (var item in items)
            {
                Bounds itemBounds = new Bounds(Hierarchy.ToWorld(item.Pose.position), _model.Bounds.dimensions);
                if (anyFingerTipTouching(itemBounds, leftHand, rightHand))
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
        private bool anyFingerTipTouching(Bounds bounds, Hand leftHand, Hand rightHand)
        {
            // Only consider tracked hands.
            // Untracked hands can still have a position, even if not visible!
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

        private void createNewItem()
        {
            // This new item will appear directly in front of user's head
            var newItemPosition = Input.Head.position + Input.Head.Forward * 0.5f;
            var newItemDto = new ItemDto
            {
                Id = Guid.NewGuid(),
                Pose = new Pose(newItemPosition, Quat.Identity),
                Title = "NEW ITEM",
                Quantity = 1
            };

            Controller.AddItem(newItemDto);
            App.ItemService.Items.Add(newItemDto);
            App.ItemService.FocusedItem = newItemDto;
        }
    }
}
