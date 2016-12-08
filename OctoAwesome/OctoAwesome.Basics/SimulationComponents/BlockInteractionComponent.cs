﻿using OctoAwesome.EntityComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using engenious;
using OctoAwesome.Basics.EntityComponents;

namespace OctoAwesome.Basics.SimulationComponents
{
    [EntityFilter(typeof(ControllableComponent),typeof(InventoryComponent))]
    public class BlockInteractionComponent : SimulationComponent<ControllableComponent,InventoryComponent>
    {
        private Simulation simulation;

        public BlockInteractionComponent(Simulation simulation)
        {
            this.simulation = simulation;
        }

        protected override bool AddEntity(Entity entity)
        {
            return true;
        }

        protected override void RemoveEntity(Entity entity)
        {

        }

        protected override void UpdateEntity(GameTime gameTime, Entity entity, ControllableComponent controller, InventoryComponent inventory)
        {
            var toolbar = entity.Components.GetComponent<ToolBarComponent>();

            if (controller.InteractBlock.HasValue)
            {
                ushort lastBlock = entity.Cache.GetBlock(controller.InteractBlock.Value);
                entity.Cache.SetBlock(controller.InteractBlock.Value, 0);

                if (lastBlock != 0)
                {
                    var blockDefinition = simulation.ResourceManager.DefinitionManager.GetBlockDefinitionByIndex(lastBlock);

                    var slot = inventory.Inventory.FirstOrDefault(s => s.Definition == blockDefinition);

                    // Wenn noch kein Slot da ist oder der vorhandene voll, dann neuen Slot
                    if (slot == null)
                    {
                        slot = new InventorySlot()
                        {
                            Definition = blockDefinition,
                            Amount = 0
                        };
                        inventory.Inventory.Add(slot);

                        
                        if (toolbar != null)
                        {
                            for (int i = 0; i < toolbar.Tools.Length; i++)
                            {
                                if (toolbar.Tools[i] == null)
                                {
                                    toolbar.Tools[i] = slot;
                                    break;
                                }
                            }
                        }
                    }
                    slot.Amount += 125;
                }
                controller.InteractBlock = null;
            }

            if (toolbar != null && controller.ApplyBlock.HasValue)
            {
                if (toolbar.ActiveTool != null)
                {
                    Index3 add = new Index3();
                    switch (controller.ApplySide)
                    {
                        case OrientationFlags.SideWest: add = new Index3(-1, 0, 0); break;
                        case OrientationFlags.SideEast: add = new Index3(1, 0, 0); break;
                        case OrientationFlags.SideSouth: add = new Index3(0, -1, 0); break;
                        case OrientationFlags.SideNorth: add = new Index3(0, 1, 0); break;
                        case OrientationFlags.SideBottom: add = new Index3(0, 0, -1); break;
                        case OrientationFlags.SideTop: add = new Index3(0, 0, 1); break;
                    }

                    if (toolbar.ActiveTool.Definition is IBlockDefinition)
                    {
                        IBlockDefinition definition = toolbar.ActiveTool.Definition as IBlockDefinition;

                        Index3 idx = controller.ApplyBlock.Value + add;
                        var boxes = definition.GetCollisionBoxes(entity.Cache, idx.X, idx.Y, idx.Z);

                        bool intersects = false;
                        var positioncomponent = entity.Components.GetComponent<PositionComponent>();
                        var bodycomponent = entity.Components.GetComponent<BodyComponent>();

                        if (positioncomponent != null && bodycomponent != null)
                        {
                            float gap = 0.01f;
                            var playerBox = new BoundingBox(
                                new Vector3(
                                    positioncomponent.Position.GlobalBlockIndex.X + positioncomponent.Position.BlockPosition.X - bodycomponent.Radius + gap,
                                    positioncomponent.Position.GlobalBlockIndex.Y + positioncomponent.Position.BlockPosition.Y - bodycomponent.Radius + gap,
                                    positioncomponent.Position.GlobalBlockIndex.Z + positioncomponent.Position.BlockPosition.Z + gap),
                                new Vector3(
                                    positioncomponent.Position.GlobalBlockIndex.X + positioncomponent.Position.BlockPosition.X + bodycomponent.Radius - gap,
                                    positioncomponent.Position.GlobalBlockIndex.Y + positioncomponent.Position.BlockPosition.Y + bodycomponent.Radius - gap,
                                    positioncomponent.Position.GlobalBlockIndex.Z + positioncomponent.Position.BlockPosition.Z + bodycomponent.Height - gap)
                                );

                            // Nicht in sich selbst reinbauen
                            
                            foreach (var box in boxes)
                            {
                                var newBox = new BoundingBox(idx + box.Min, idx + box.Max);
                                if (newBox.Min.X < playerBox.Max.X && newBox.Max.X > playerBox.Min.X &&
                                    newBox.Min.Y < playerBox.Max.Y && newBox.Max.X > playerBox.Min.Y &&
                                    newBox.Min.Z < playerBox.Max.Z && newBox.Max.X > playerBox.Min.Z)
                                    intersects = true;
                            }
                        }
                       

                        if (!intersects)
                        {
                            entity.Cache.SetBlock(idx, simulation.ResourceManager.DefinitionManager.GetBlockDefinitionIndex(definition));

                            toolbar.ActiveTool.Amount -= 125;
                            if (toolbar.ActiveTool.Amount <= 0)
                            {
                                inventory.Inventory.Remove(toolbar.ActiveTool);
                                for (int i = 0; i < toolbar.Tools.Length; i++)
                                {
                                    if (toolbar.Tools[i] == toolbar.ActiveTool)
                                        toolbar.Tools[i] = null;
                                }
                                toolbar.ActiveTool = null;
                            }
                        }
                    }
                    controller.ApplyBlock = null;
                }
            }
        }
    }
}
