using System;
using UnityEngine;

namespace UiParticles
{
    public struct ParticleTrailData
    {
        private const int ParticleTrailLengh = 1000;
        public uint ParticleId;
        public bool NotFree;
        public Vector3[] Positions;
        public int PositionsCount;

        public ParticleTrailData(uint particleId)
        {
            ParticleId = particleId;
            NotFree = false;
            Positions = new Vector3[ParticleTrailLengh];
            PositionsCount = 0;
        }

        public ParticleTrailData(uint particleId, bool notFree)
        {
            ParticleId = particleId;
            NotFree = notFree;
            Positions = new Vector3[ParticleTrailLengh];
            PositionsCount = 0;
        }

        public void AddPosition(Vector3 position)
        {
            Positions[PositionsCount] = position;
            PositionsCount++;
        }

        public Vector3 GetLastPosition()
        {
            if(PositionsCount<=0)
                throw new Exception("Postions empty");
            return Positions[PositionsCount - 1];
        }
    }
}