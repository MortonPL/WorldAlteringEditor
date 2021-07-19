﻿using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that moves a game object on the map.
    /// </summary>
    public class MoveObjectMutation : Mutation
    {
        public MoveObjectMutation(IMutationTarget mutationTarget, GameObject gameObject, Point2D newPosition) : base(mutationTarget)
        {
            this.gameObject = gameObject;
            oldPosition = gameObject.Position;
            this.newPosition = newPosition;
        }

        private GameObject gameObject;
        private Point2D oldPosition;
        private Point2D newPosition;

        private void MoveObject(Point2D position)
        {
            switch (gameObject.WhatAmI())
            {
                case RTTIType.Aircraft:
                    MutationTarget.Map.MoveAircraft((Aircraft)gameObject, position);
                    break;
                case RTTIType.Building:
                    MutationTarget.Map.MoveBuilding((Structure)gameObject, position);
                    break;
                case RTTIType.Unit:
                    MutationTarget.Map.MoveUnit((Unit)gameObject, position);
                    break;
                case RTTIType.Infantry:
                    MutationTarget.Map.MoveInfantry((Infantry)gameObject, position);
                    break;
                case RTTIType.Terrain:
                    MutationTarget.Map.MoveTerrainObject((TerrainObject)gameObject, position);
                    break;
            }

            MutationTarget.AddRefreshPoint(newPosition);
            MutationTarget.AddRefreshPoint(oldPosition);
        }

        public override void Perform()
        {
            // TODO handle sub-cell for infantry
            MoveObject(newPosition);
        }

        public override void Undo()
        {
            // TODO handle sub-cell for infantry
            MoveObject(oldPosition);
        }
    }
}
