using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace UnitSpriteStudio.Shading {
	class NormalMap {
		internal enum EHeightCalculation {
			Rational,
			Fixed
		}

		internal Vector3D[,] Map;
		Selection Area;

		// Settings
		internal double SurfaceCurvePower=0.5;
		internal EHeightCalculation HeightCalculation =EHeightCalculation.Rational;
		internal float HeightParameter=2;
		public NormalMap(Selection area) {
			this.Area = area;
		}

		public void Generate() {
			float[,] heightMap = new float[Area.SizeX, Area.SizeY];
			heightMap = CalculateHeightMap();
			CalculateNormalMap(heightMap);
		}

		private void CalculateNormalMap(float[,] heightMap) {
			Map = new Vector3D[Area.SizeX, Area.SizeY];
			Vector3D v2 = new Vector3D(), v4 = new Vector3D(), v6 = new Vector3D(), v8 = new Vector3D();
			for (int x = 0; x < Area.SizeX; x++) {
				for (int y = 0; y < Area.SizeY; y++) {
					if (Area.GetPoint(x, y)) {
						if (Area.GetPoint(x - 1, y)) {
							v4 = new Vector3D(-1, 0, heightMap[x - 1, y] - heightMap[x, y]);
						}
						if (Area.GetPoint(x + 1, y)) {
							v6 = new Vector3D(1, 0, heightMap[x + 1, y] - heightMap[x, y]);
						}
						if (Area.GetPoint(x, y + 1)) {
							v2 = new Vector3D(0, 1, heightMap[x, y + 1] - heightMap[x, y]);
						}
						if (Area.GetPoint(x, y - 1)) {
							v8 = new Vector3D(0, -1, heightMap[x, y - 1] - heightMap[x, y]);
						}
						int averagedVectors = 0;
						Vector3D normal = new Vector3D();
						if (Area.GetPoint(x - 1, y) && Area.GetPoint(x, y + 1)) {
							averagedVectors++;
							Vector3D v24 = Vector3D.CrossProduct(v2, v4);
							normal = v24;
						}
						if (Area.GetPoint(x - 1, y) && Area.GetPoint(x, y - 1)) {
							averagedVectors++;
							Vector3D v48 = Vector3D.CrossProduct(v4, v8);
							if (normal.Z == 0) {
								normal = v48;
							} else {
								normal = Vector3D.Add(normal, v48);
							}
						}
						if (Area.GetPoint(x + 1, y) && Area.GetPoint(x, y - 1)) {
							averagedVectors++;
							Vector3D v86 = Vector3D.CrossProduct(v8, v6);
							if (normal.Z == 0) {
								normal = v86;
							} else {
								normal = Vector3D.Add(normal, v86);
							}
						}
						if (Area.GetPoint(x + 1, y) && Area.GetPoint(x, y + 1)) {
							averagedVectors++;
							Vector3D v62 = Vector3D.CrossProduct(v6, v2);
							if (normal.Z == 0) {
								normal = v62;
							} else {
								normal = Vector3D.Add(normal, v62);
							}
						}
						normal = Vector3D.Divide(normal, averagedVectors);
						normal.Normalize();
						Map[x, y] = normal;
					}
				}
			}
		}

		private float[,] CalculateHeightMap() {
			float[,] distanceMap = new float[Area.SizeX, Area.SizeY];
			float[,] heightMap = new float[Area.SizeX, Area.SizeY];
			float furthestEdge = 0;

			for (int x = 0; x < Area.SizeX; x++) {
				for (int y = 0; y < Area.SizeY; y++) {
					if (Area.GetPoint(x, y)) {
						float nearestEdgeDistance = 0;
						int ex, ey;
						// Orthogonals
						for (ex = x + 1; ex < Area.SizeX && Area.GetPoint(ex, y); ex++) { }
						nearestEdgeDistance = ex - x;
						for (ex = x - 1; ex >= 0 && Area.GetPoint(ex, y); ex--) { }
						nearestEdgeDistance = Math.Min(x - ex, nearestEdgeDistance);
						for (ey = y + 1; ey < Area.SizeY && Area.GetPoint(x, ey); ey++) { }
						nearestEdgeDistance = Math.Min(ey - y, nearestEdgeDistance);
						for (ey = y - 1; ey >= 0 && Area.GetPoint(x, ey); ey--) { }
						nearestEdgeDistance = Math.Min(y - ey, nearestEdgeDistance);
						// Diagonals
						for (ex = x + 1, ey = y + 1; ex < Area.SizeX && ey < Area.SizeY && Area.GetPoint(ex, ey); ex++, ey++) { }
						nearestEdgeDistance = Math.Min((float)Math.Sqrt((ex - x) * (ex - x) + (ey - y) * (ey - y)), nearestEdgeDistance);
						for (ex = x - 1, ey = y + 1; ex >= 0 && ey < Area.SizeY && Area.GetPoint(ex, ey); ex--, ey++) { }
						nearestEdgeDistance = Math.Min((float)Math.Sqrt((x - ex) * (x - ex) + (ey - y) * (ey - y)), nearestEdgeDistance);
						for (ex = x - 1, ey = y - 1; ex >= 0 && ey >= 0 && Area.GetPoint(ex, ey); ex--, ey--) { }
						nearestEdgeDistance = Math.Min((float)Math.Sqrt((x - ex) * (x - ex) + (y - ey) * (y - ey)), nearestEdgeDistance);
						for (ex = x + 1, ey = y - 1; ex < Area.SizeX && ey >= 0 && Area.GetPoint(ex, ey); ex++, ey--) { }
						nearestEdgeDistance = Math.Min((float)Math.Sqrt((ex - x) * (ex - x) + (y - ey) * (y - ey)), nearestEdgeDistance);

						distanceMap[x, y] = nearestEdgeDistance;
						if (furthestEdge < nearestEdgeDistance) furthestEdge = nearestEdgeDistance;
					}
				}
			}

			for (int x = 0; x < Area.SizeX; x++) {
				for (int y = 0; y < Area.SizeY; y++) {
					if (Area.GetPoint(x, y)) {
						distanceMap[x, y] = distanceMap[x, y] / furthestEdge;
						if (HeightCalculation == EHeightCalculation.Fixed) {
							heightMap[x, y] = (float)Math.Pow(distanceMap[x, y], SurfaceCurvePower) * (HeightParameter);
						} else {
							heightMap[x, y] = (float)Math.Pow(distanceMap[x, y], SurfaceCurvePower) * (furthestEdge / HeightParameter);
						}
					}
				}
			}
			return heightMap;
		}

	}
}
