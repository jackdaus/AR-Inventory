using ARInventory.Entities.Models;
using StereoKit;
using StereoKit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARInventory
{
    internal class ManageInventory : IStepper
    {
        public bool Enabled { get; set; }

        Pose _pose = new Pose(0.2f, 0, -0.4f, Quat.LookDir(0, 0, 1));
        
        Model _model;
        List<ItemDto> _visibleItems = new List<ItemDto>();
        ItemDto _focusedItem;

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
                bool toggle = App.Passthrough.EnabledPassthrough;
                if (UI.ButtonRound("Passthrough", Catalog.Sprites.IconEye))
                    App.Passthrough.EnabledPassthrough = !App.Passthrough.EnabledPassthrough;
            }

            if (UI.Button("New item"))
            {
                createNewItem();
            }

            if (UI.Button("Load all items"))
            {
                loadAllItems();
            }

            UI.WindowEnd();

            // Render Items
            //
            _visibleItems.ForEach(item =>
            {
                if (UI.Handle(item.Id.ToString(), ref item.Pose, _model.Bounds))
                {
                    // TODO this could be made more efficient by only updating once the UI handle is released
                    Factory.UpdateItemPose(item);
                }

                _model.Draw(item.Pose.ToMatrix());

                // Item label floats 10cm above the object
                Vec3 textPosition = item.Pose.position;
                textPosition.y += 10 * U.cm;

                Quat textOrientation = Quat.LookAt(textPosition, Input.Head.position);
                Color textColor = _focusedItem == item ? Color.White : Color.Black;
                Text.Add(item.Title, Matrix.TR(textPosition, textOrientation), textColor);

                if (_focusedItem == item)
                {
                    // TODO set UIBox scale to adjust to smaller/larger models
                    Mesh.Cube.Draw(Material.UIBox, item.Pose.ToMatrix(0.12f));
                }
            });


            // Update focused item
            //
            if (_focusedItem == null)
            {
                _focusedItem = firstItemTouchedByFinger(_visibleItems, lHand, rHand);
            }
            else
            {
                Bounds focusedItemBounds = new Bounds(_focusedItem.Pose.position, _model.Bounds.dimensions);
                bool stillTouchingFocusedItem = anyFingerTipTouching(focusedItemBounds, lHand, rHand);

                if (!stillTouchingFocusedItem)
                {
                    var touchedItem = firstItemTouchedByFinger(_visibleItems, lHand, rHand);
                    if (touchedItem != null && touchedItem != _focusedItem)
                    {
                        // Touching a different item this frame. Make it the new focused item and make a sound
                        _focusedItem = touchedItem;
                        Sound.Click.Play(touchedItem.Pose.position);
                    }
                }
            }
        }

        private ItemDto firstItemTouchedByFinger(List<ItemDto> items, Hand leftHand, Hand rightHand)
        {
            foreach (var item in items)
            {
                Bounds itemBounds = new Bounds(item.Pose.position, _model.Bounds.dimensions);
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

            Factory.AddItem(newItemDto);
            _visibleItems.Add(newItemDto);
            _focusedItem = newItemDto;
        }

        private void loadAllItems()
        {
            _visibleItems = Factory.GetItemDtos().ToList();
        }
    }
}
