﻿//
// Author  : Oliver Brodhage
// Company : Decentralised Team of Developers
//

using System;
using UnityEngine;

namespace DragginzVoxelWorldEditor
{
    /// <summary>
    /// class for creating Box primitive
    /// </summary>
    public class VoxelChunkMesh
    {
        //private static bool dbg;

        /// <summary>
        /// generate mesh geometry for box
        /// </summary>
        /// <param name="mesh">mesh to be generated</param>
        /// <param name="width">width of cube</param>
        /// <param name="height">height of cube</param>
        /// <param name="depth">depth of cube</param>
        /// <param name="widthSegments">number of triangle segments in width direction</param>
        /// <param name="heightSegments">number of triangle segments in height direction</param>
        /// <param name="depthSegments">number of triangle segments in depth direction</param>
        /// <param name="cubeMap">enable 6-sides cube map uv mapping</param>
        /// <param name="edgeOffsets">offsets on edges for creating a ramp</param>
        /// <param name="flipUV">flag to flip uv mapping</param>
        /// <param name="pivot">position of the model pivot</param>
		public static void create(Mesh mesh, float width, float height, float depth, int widthSegments, int heightSegments, int depthSegments, bool cubeMap) //, float[] edgeOffsets, bool flipUV)
        {
            width  = Mathf.Clamp(width,  0, 100);
            height = Mathf.Clamp(height, 0, 100);
            depth  = Mathf.Clamp(depth,  0, 100);
            heightSegments = Mathf.Clamp(heightSegments, 1, 100);
            widthSegments  = Mathf.Clamp(widthSegments,  1, 100);
            depthSegments  = Mathf.Clamp(depthSegments,  1, 100);

			//Debug.Log ("createcube mesh - w, h, d, segW, segH, segD: "+width+", "+height+", "+depth+", "+widthSegments+", "+heightSegments+", "+depthSegments);

            mesh.Clear();

            int numTriangles = widthSegments*depthSegments*6 +
                               widthSegments*heightSegments*6 +
                               depthSegments*heightSegments*6;

            int numVertices = (widthSegments + 1)*(depthSegments + 1) +
                              (widthSegments + 1)*(heightSegments + 1) +
                              (depthSegments + 1)*(heightSegments + 1);

            numTriangles *= 2;
            numVertices *= 2;

            Vector3 pivotOffset = Vector3.zero;
            /*switch (pivot)
            {
                case PivotPosition.Top: pivotOffset = new Vector3(0.0f, -height/2, 0.0f);
                    break;
                case PivotPosition.Botttom: pivotOffset = new Vector3(0.0f, height/2, 0.0f);
                    break;
            }*/

            if (numVertices > 65000)
            {
				UnityEngine.Debug.LogError("Too many vertices: "+numVertices);
				return;
            }

			Vector3[] vertices = new Vector3[numVertices];
			Vector2[] uvs = new Vector2[numVertices];
			int[] triangles = new int[numTriangles];

            int vertIndex = 0;
            int triIndex = 0;

            var a0 = new Vector3(-width / 2, pivotOffset.y - height / 2, -depth / 2);
            var b0 = new Vector3(-width / 2, pivotOffset.y - height / 2, depth / 2);
            var c0 = new Vector3(width / 2, pivotOffset.y - height / 2, depth / 2);
            var d0 = new Vector3(width / 2, pivotOffset.y - height / 2, -depth / 2);

            var a1 = new Vector3(-width / 2, height / 2 + pivotOffset.y, -depth / 2);
            var b1 = new Vector3(-width / 2, height / 2 + pivotOffset.y, depth / 2);
            var c1 = new Vector3(width / 2, height / 2 + pivotOffset.y, depth / 2);
            var d1 = new Vector3(width / 2, height / 2 + pivotOffset.y, -depth / 2);

            /*if (edgeOffsets != null && edgeOffsets.Length > 3)
            {
                b1.x += edgeOffsets[0];
                a1.x += edgeOffsets[0];
                b0.x += edgeOffsets[1];
                a0.x += edgeOffsets[1];

                c0.x += edgeOffsets[3];
                c1.x += edgeOffsets[2];
                d0.x += edgeOffsets[3];
                d1.x += edgeOffsets[2];
            }*/

			CreatePlane(0, a0, b0, c0, d0, widthSegments, depthSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, depth, width);
			CreatePlane(1, b1, a1, d1, c1, widthSegments, depthSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, depth, width);

			CreatePlane(2, b0, b1, c1, c0, widthSegments, heightSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, height, width);
			CreatePlane(3, d0, d1, a1, a0, widthSegments, heightSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, height, width);

			CreatePlane(4, a0, a1, b1, b0, depthSegments, heightSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, height, depth);
			CreatePlane(5, c0, c1, d1, d0, depthSegments, heightSegments, cubeMap, ref vertices, ref uvs, ref triangles, ref vertIndex, ref triIndex, height, depth);

            /*if (flipUV)
            {
                for (var i = 0; i < uvs.Length; i++)
                {
                    uvs[i].x = 1.0f - uvs[i].x;
                }
            }*/

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;

			// Editor only :( -> MeshUtility.Optimize(mesh);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

		static void CreatePlane(int id, Vector3 a, Vector3 b, Vector3 c, Vector3 d, int segX, int segY, bool cubeMap, ref Vector3[] vertices, ref Vector2[] uvs, ref int[] triangles, ref int vertIndex, ref int triIndex, float width, float height)
        {
			var uvFactorX = 1.0f / segX;
			var uvFactorY = 1.0f / segY;

			float wPercentage = (width / VoxelUtils.CHUNK_SIZE) / (float)VoxelUtils.MAX_CHUNK_UNITS;
			float fUVX = wPercentage / (float)segX;
			float hPercentage = (height / VoxelUtils.CHUNK_SIZE) / (float)VoxelUtils.MAX_CHUNK_UNITS;
			float fUVY = hPercentage / (float)segY;

			//float oneUnit = 1.0f / (float)VoxelUtils.MAX_CHUNK_UNITS;
			//float fUVX = (width  / VoxelUtils.CHUNK_SIZE) / (float)segX * oneUnit; //0.01388889f;//(float)segX / 72.0f;// / (float)segX; //0.01388889f;//
			//float fUVY = (height / VoxelUtils.CHUNK_SIZE) / (float)segY * oneUnit; //0.01388889f;//(float)segY / 72.0f;// / (float)segY; //0.01388889f;//
			//Debug.Log(fUVX+", "+fUVY);

			Vector3 vDown = d - a;
			Vector3 vUp = c - b;

            int vertOffset = vertIndex;

			Vector3 pDown, pUp, v;
			float fX, fY;

			for (fY = 0.0f; fY < segY+1; fY++)
            {
				for (fX = 0.0f; fX < segX+1; fX++)
                {
                    pDown = a + vDown*fY*uvFactorY;
                    pUp = b + vUp*fY*uvFactorY;

                    v = pDown + (pUp - pDown)*fX*uvFactorX;

                    vertices[vertIndex] = v;
					uvs[vertIndex] = new Vector2 (fX * fUVX, fY * fUVY); //new Vector2 (x * uvFactorX, y * uvFactorY);//

					/*if (cubeMap)
                    {
                        uvs[vertIndex] = GetCube6UV(id/2, id%2, uvFactor);
                    }
                    else
                    {
                        uvs[vertIndex] = uvFactor;
                    }*/

                    vertIndex++;
                }
            }

            var hCount2 = segX + 1;

			int x, y;
            for (y = 0; y < segY; y++)
            {
                for (x = 0; x < segX; x++)
                {
                    triangles[triIndex + 0] = vertOffset + (y * hCount2) + x;
                    triangles[triIndex + 1] = vertOffset + ((y + 1) * hCount2) + x;
                    triangles[triIndex + 2] = vertOffset + (y * hCount2) + x + 1;

                    triangles[triIndex + 3] = vertOffset + ((y + 1) * hCount2) + x;
                    triangles[triIndex + 4] = vertOffset + ((y + 1) * hCount2) + x + 1;
                    triangles[triIndex + 5] = vertOffset + (y * hCount2) + x + 1;
                    triIndex += 6;
                }
            }
        }

        /// <summary>
        /// generate uv coordinates for a texture with 6 sides of the box
        /// </summary>
        static Vector2 GetCube6UV(int sideID, int paralel, Vector2 factor)
        {
            factor.x = factor.x*0.3f;
            factor.y = factor.y*0.5f;

            switch (sideID)
            {
                case 0:
                    if (paralel == 0)
                    {
                        factor.y += 0.5f;
                        return factor;
                    }
                    else
                    {
                        factor.y += 0.5f;
                        factor.x += 2.0f / 3;
                        return factor;
                    }
                case 1:
                    if (paralel == 0)
                    {
                        factor.x += 1.0f / 3;
                        return factor;
                    }
                    else
                    {
                        factor.x += 2.0f / 3;
                        return factor;
                    }
                case 2:
                    if (paralel == 0)
                    {
                        factor.y += 0.5f;
                        factor.x += 1.0f / 3;
                        return factor;
                    }
                    else
                    {
                        return factor;
                    }
            }

            return Vector2.zero;
        }
	}
}