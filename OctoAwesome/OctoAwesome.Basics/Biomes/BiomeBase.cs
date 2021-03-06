﻿using OctoAwesome.Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OctoAwesome.Basics.Biomes
{
    public abstract class BiomeBase : IBiome
    {
        public IPlanet Planet { get; private set; }

        public List<IBiome> SubBiomes { get; protected set; }

        public INoise BiomeNoiseGenerator { get; protected set; }

        public float MinValue { get; protected set; }

        public float MaxValue { get; protected set; }

        public float ValueRangeOffset { get; protected set; }

        public float ValueRange { get; protected set; }

        public BiomeBase(IPlanet planet, float minValue, float maxValue, float valueRangeOffset, float valueRange)
        {
            SubBiomes = new List<IBiome>();
            Planet = planet;
            MinValue = minValue;
            MaxValue = maxValue;
            ValueRangeOffset = valueRangeOffset;
            ValueRange = valueRange;
        }

        public virtual float[,] GetHeightmap(Index2 chunkIndex)
        {
            float[,] values = new float[Chunk.CHUNKSIZE_X, Chunk.CHUNKSIZE_Y];

            chunkIndex = new Index2(chunkIndex.X * Chunk.CHUNKSIZE_X, chunkIndex.Y * Chunk.CHUNKSIZE_Y);

            float[,] heights = BiomeNoiseGenerator.GetTileableNoiseMap2D(chunkIndex.X, chunkIndex.Y, Chunk.CHUNKSIZE_X, Chunk.CHUNKSIZE_Y, Planet.Size.X * Chunk.CHUNKSIZE_X, Planet.Size.Y * Chunk.CHUNKSIZE_Y);

            for (int x = 0; x < Chunk.CHUNKSIZE_X; x++)
            {
                for (int y = 0; y < Chunk.CHUNKSIZE_Y; y++)
                {
                    values[x, y] = (heights[x, y] / 2 + 0.5f) * ValueRange + ValueRangeOffset;
                }
            }

            return values;
        }
    }
}
