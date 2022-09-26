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
        
        List<ItemDto> _visibleItems = new List<ItemDto>();
        Model _model;

        public bool Initialize()
        {
            _model = Model.FromMesh(
                Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
                Default.MaterialUI);

            return true;
        }

        public void Shutdown()
        {
            
        }

        public void Step()
        {
            UI.WindowBegin("manage-inventory-window", ref _pose, UIWin.Body);
            if (UI.Button("New item"))
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
            }

            if (UI.Button("Load all items"))
            {
                loadAllItems();
            }

            UI.WindowEnd();

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
                textPosition.y += 0.1f;

                Quat textOrientation = Quat.LookAt(textPosition, Input.Head.position);
                Text.Add(item.Title, Matrix.TR(textPosition, textOrientation));
            });
        }

        private void loadAllItems()
        {
            _visibleItems = Factory.GetItemDtos().ToList();
        }
    }
}
