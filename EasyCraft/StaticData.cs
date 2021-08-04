﻿using System.Collections;
using System.Collections.Generic;
using EasyCraft.engine;
using SharpDX;

namespace EasyCraft
{
	public static class StaticData
	{
		public static readonly int ChunkWidth = 16;
		public static readonly int ChunkHeight = 256;

		public static readonly Vector3[] voxelVerts = new Vector3[] {
			new Vector3(0.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 1.0f, 0.0f),
			new Vector3(0.0f, 1.0f, 0.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(1.0f, 0.0f, 1.0f),
			new Vector3(1.0f, 1.0f, 1.0f),
			new Vector3(0.0f, 1.0f, 1.0f),
		};

		public static readonly Vector3[] faceChecks = new Vector3[] {
			new Vector3(0.0f, 0.0f, -1.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(0.0f, 1.0f, 0.0f),
			new Vector3(0.0f, -1.0f, 0.0f),
			new Vector3(-1.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 0.0f, 0.0f)
		};

		public static readonly Vector3[] aoChecks = new Vector3[] {
			new Vector3(0.0f, 0.0f, -1.0f),
			new Vector3(0.0f, 0.0f, 1.0f),
			new Vector3(-1.0f, 0.0f, -1.0f),
			new Vector3(-1.0f, 0.0f, 1.0f),
			new Vector3(-1.0f, 0.0f, 0.0f),
			new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, -1.0f),
            new Vector3(1.0f, 0.0f, 1.0f)
        };

		public static readonly int[,] aoIndices = new int[,] {
			{0,2},
			{1,3},
			{0,-1},
			{1,-1},
			{0,1},
			{2,3},
			{2,-1},
			{3,-1}
		};

		public static readonly int[,] voxelTris = new int[,] {
			// 0 1 2 2 1 3
			{0, 3, 1, 2}, // Back Face
			{5, 6, 4, 7}, // Front Face
			{3, 7, 2, 6}, // Top Face
			{1, 5, 0, 4}, // Bottom Face
			{4, 7, 0, 3}, // Left Face
			{1, 2, 5, 6} // Right Face
		};

		public static readonly Vector2[] voxelUvs = new Vector2[] {
			new Vector2 (0.0f, 0.0f),
			new Vector2 (0.0f, 1.0f),
			new Vector2 (1.0f, 0.0f),
			new Vector2 (1.0f, 1.0f)
		};
	}
}