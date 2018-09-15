using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using MapIndex;
using Rand = UnityEngine.Random;

/// <summary>
/// PathGenerator는 맵 위의 두 지점 사이를 이동하는 경로를 생성합니다.
/// PathGenerator는 A* 알고리즘으로 작성되었습니다.
/// GetPath을 통해 두 지점 사이를 이동하는 경로를 List 형태로 받을 수 있습니다.
/// AvailableToMove을 통해 어떤 지점이 실제로 존재하는지 알 수 있습니다.
/// 정상 작동을 위해 GameObject에 Map 요소가 있어야 합니다.
/// </summary>
public class PathGenerator : MonoBehaviour {
    #region Component
    // rows, cols       맵의 행렬 크기
    // pivot            맵의 시작 위치
    // map              맵의 각 타일에 대한 정보
    int rows, cols;
    Vector2 pivot;
    TileState[,] map;

    // AbleToMove       타일에서 원하는 방향으로 진행할 수 있는지 판별한다.
    // IsValid          타일이 유효한지 판별한다.
    bool AbleToMove (Index idx, int dir) {
        return IsValid(idx.NextIndex(dir)) && (dir % 2 == 1 ||
                       (IsValid(idx.NextIndex(dir + 1)) && IsValid(idx.NextIndex(dir + 7))));
    }

    bool IsValid (Index idx) {
        return 0 <= idx.row && idx.row < rows
            && 0 <= idx.col && idx.col < cols
                && map[idx.row, idx.col] != TileState.Hole;
    }

    // TileState        타일의 상태
    // Node             길을 찾을 때 각 타일에 대한 부가정보를 저장하는 구조체
    // NodeComparer     길을 찾을 때 SortedList<Index>에 쓰이는 비교 클래스
    // DistanceMethod   어떤 지점으로부터 최종 목적지까지 거리를 구하는 함수형
    enum TileState { Normal, Hole, Tree, Portal };

    struct Node {
        public float degree;
        public float distance;
        public float ValueFactor { get { return degree + distance; } }
        public Index parent;
        public int pathDirection;
        public Node (float deg, float dist, Index from, int dir) {
            degree = deg;
            distance = dist;
            parent = from;
            pathDirection = dir;
        }
    }

    class NodeComparer : IComparer<double> {
        public int Compare (double x, double y) {
            return x.CompareTo(y) == 0 ? 1 : x.CompareTo(y);
        }
    }

    delegate float DistanceMethod (Index a);
    #endregion

    #region Initialize
    void Start () {
		Map m = GetComponent<Map>();
		
		rows = m.row;
		cols = m.column;
		
		pivot = m.startingPoint;

        m.LoadField();

		bool[,] _hole   = m.holes;
		bool[,] _tree   = m.trees;
		bool[,] _portal = m.portals;
		
		map = new TileState[rows, cols];
		
		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++)
				map[i, j] = _hole  [i, j] ? TileState.Hole
					      : _tree  [i, j] ? TileState.Tree
					      : _portal[i, j] ? TileState.Portal
					      : TileState.Normal;
	}
    #endregion
    
    public List<Vector2> GetPath(Vector2 playerPosition, Vector2 targetPosition) {
        #region Initialize
        Index playerIdx = playerPosition.IndexPosition(pivot);
		Index targetIdx  = targetPosition.IndexPosition(pivot);
		
		DistanceMethod DistanceToTarget = (Index idx) => 
			(targetPosition - idx.VectorPosition(pivot)).magnitude;
		
		// openNode: open node list by index, sorted by Value factor
		var openNode = new SortedList<double, Index>(new NodeComparer());
		// closeNode: close node list by index
		var closeNode = new HashSet<Index>();
		// nodeData: information of every nodes
		var nodeData = new Dictionary<Index, Node>();
		
		nodeData[playerIdx] = new Node(0, DistanceToTarget(playerIdx), new Index(-1, -1), -1);
		openNode.Add(nodeData[playerIdx].ValueFactor, playerIdx);
        #endregion 

        #region Find path using A*
        while (openNode.Count > 0) {
			Index c = openNode.Values[0];
			openNode.RemoveAt(0);
			closeNode.Add(c);
			
			Node c_data = nodeData[c];
			
			if (c.row == targetIdx.row && c.col == targetIdx.col)
				break;
			
			for (int i = 0; i < 8; i++) 
			if (AbleToMove(c, i)) {
				Index n = c.NextIndex(i);
				if (!closeNode.Contains(n)) {
					Node n_data = new Node(c_data.degree + ((i % 2 == 0) ? 1.414f : 1f),
					                       DistanceToTarget(n), c, i);
					if (!openNode.ContainsValue(n)) {
						nodeData[n] = n_data;
						openNode.Add(n_data.ValueFactor, n);
					}
					else if (n_data.ValueFactor < openNode.Keys[openNode.IndexOfValue(n)]) {
						nodeData[n] = n_data;
						openNode.RemoveAt(openNode.IndexOfValue(n));
						openNode.Add(n_data.ValueFactor, n);
					}
				}
			}
		}

        if (!closeNode.Contains(targetIdx)) {
            Debug.Log("Cannot find path");
            return null;
        }
        #endregion

        #region Get path list
        List<Index> idxList = new List<Index>();
		for (Index i = targetIdx; i.row != playerIdx.row || i.col != playerIdx.col; 
		     i = nodeData[i].parent)
			idxList.Add(i);
        idxList.Add(playerIdx);
		idxList.Reverse();
		
		if (idxList == null || idxList.Count == 0)
			return null;
        #endregion

        #region Path Optimization        
        for (int i = idxList.Count - 2; i >= 0; i--)
            if (nodeData[idxList[i]].pathDirection == nodeData[idxList[i + 1]].pathDirection)
                idxList.RemoveAt(i);

        for (int i = idxList.Count - 3; i >= 0; i--) {
            // get all tiles behind path
            Index a = idxList[i].Rotated, b = idxList[i + 2].Rotated;
            HashSet<Index> set = new HashSet<Index>();
            set.Add(a);
            set.Add(b);
            int dr = Math.Abs(a.row - b.row), dc = Math.Abs(a.col - b.col);
            int drf = ((a.row - b.row > 0)? 1 : -1), dcf = ((a.col - b.col > 0)? 1 : -1);
            if (dr > dc) { // dr > dc
                for (int j = 0; j < dr; j++) {
                    float tcf = (j + 0.5f) / dr * dc + 0.5f;
                    int tc = (int)tcf;
                    set.Add(new Index(b.row + (j    ) * drf, b.col + tc * dcf));
                    set.Add(new Index(b.row + (j + 1) * drf, b.col + tc * dcf));
                    if (tcf - Math.Floor(tcf) < 0.3f) {
                        set.Add(new Index(b.row + (j    ) * drf, b.col + (tc - 1) * dcf));
                        set.Add(new Index(b.row + (j + 1) * drf, b.col + (tc - 1) * dcf));
                    }
                    if (tcf - Math.Floor(tcf) > 0.7f) {
                        set.Add(new Index(b.row + (j    ) * drf, b.col + (tc + 1) * dcf));
                        set.Add(new Index(b.row + (j + 1) * drf, b.col + (tc + 1) * dcf));
                    }
                }
            }
            else if (dr < dc) { // dr < dc
                for (int j = 0; j < dc; j++) {
                    float trf = (j + 0.5f) / dc * dr + 0.5f;
                    int tr = (int)trf;
                    set.Add(new Index(b.row + tr * drf, b.col + (j    ) * dcf));
                    set.Add(new Index(b.row + tr * drf, b.col + (j + 1) * dcf));
                    if (trf - Math.Floor(trf) < 0.3f) {
                        set.Add(new Index(b.row + (tr - 1) * drf, b.col + (j    ) * dcf));
                        set.Add(new Index(b.row + (tr - 1) * drf, b.col + (j + 1) * dcf));
                    }
                    if (trf - Math.Floor(trf) > 0.7f) {
                        set.Add(new Index(b.row + (tr + 1) * drf, b.col + (j    ) * dcf));
                        set.Add(new Index(b.row + (tr + 1) * drf, b.col + (j + 1) * dcf));
                    }
                }
            }
            else { // dr == dc
                for (int j = 0; j < dr; j++) { 
                    set.Add(new Index(b.row +  j    * drf, b.col +  j    * dcf));
                    set.Add(new Index(b.row + (j+1) * drf, b.col +  j    * dcf));
                    set.Add(new Index(b.row  + j    * drf, b.col + (j+1) * dcf));
                }
            }
            // check passable
            bool passable = true;
            foreach (Index idx in set) 
                if (!IsValid(idx.UnRotated)) {
                    passable = false;
                    break;
                }
            if (passable)
                idxList.RemoveAt(i + 1);
        }
        
        idxList.RemoveAt(0);
        #endregion

        #region Convert path to real position
        List<Vector2> vecList = idxList.Select(n => n.VectorPosition(pivot)).ToList();
		vecList.RemoveAt(vecList.Count - 1);
        vecList.Add(targetPosition);
        #endregion

        return vecList;
	}

    public bool AvailableToMove(Vector2 position) {
        return IsValid(position.IndexPosition(pivot));
    }

    public Vector2 RandomPosition(Vector2 position, int maxDistance) {
        Index currentIdx = position.IndexPosition(pivot);
        List<Index> ableList = new List<Index>();

        for (int i = currentIdx.row - maxDistance; i <= currentIdx.row + maxDistance; i++)
            for (int j = currentIdx.col - maxDistance; j <= currentIdx.col + maxDistance; j++)
                if ((i != currentIdx.row || j != currentIdx.col) && IsValid(new Index(i, j)))
                    ableList.Add(new Index(i, j));

        Vector2 size = IndexExtension.Size;

        return ableList[(int)Rand.Range(0, ableList.Count - 0.01f)].VectorPosition(pivot)
            + new Vector2(Rand.Range(-size.x / 4f, size.x / 4f), Rand.Range(-size.y / 4f, size.y / 4f));
    }

}


/// <summary>
/// Map 위의 타일 위치를 (행,열) 형태의 Index로 정의하고, 이와 관련된 기능을 제공합니다.
/// </summary>
namespace MapIndex {
	
    [Serializable]
	public struct Index {
		
		public int row, col;
		
		public Index(int row, int col) {
			this.row = row;
			this.col = col;
		}
		
		public Index NextIndex (int direction) {
			switch (direction % 8) {
			case 0: return new Index(row, col + 1);
			case 1: return new Index(row - 1, col + (row % 2));
			case 2: return new Index(row - 2, col);
			case 3: return new Index(row - 1, col - (1 - row % 2));
			case 4: return new Index(row, col - 1);
			case 5: return new Index(row + 1, col - (1 - row % 2));
			case 6: return new Index(row + 2, col);
			case 7: return new Index(row + 1, col + (row % 2));
			}
			return this;
		}

        public Index Rotated { get {
            return new Index(col + (row + 1) / 2, col - row / 2);
        }}
        public Index UnRotated { get {
            return new Index(row - col, (row + col) / 2);
        }}
		
		public static Index operator - (Index a, Index b) {
			return new Index(a.row - b.row, a.col - b.col);
		}
		
		public static Index operator + (Index a, Index b) {
			return new Index(a.row + b.row, a.col + b.col);
		}

        public override string ToString () {
            return "(" + row + ", " + col + ")";
        }
    }
	

	//TODO : Extension 함수가 바뀌면 Editor/pfyStageEditor.cs의 setPosition도 바꿔야 함
	public static class IndexExtension {
		
		static Vector2 size;
		static float pixelResolution = 100f;
		
		static IndexExtension() {
			Texture _tex;
			//TODO : Texture 크기를 알기 위해 로드합니다 ; resource가 바뀌면 바꿔야 함
			if (_tex = (Texture)Resources.Load("In-Game/Tile/1_0", typeof(Texture))) {
				size = new Vector2(_tex.width / pixelResolution, _tex.height / pixelResolution);
			}
			else {
				Debug.Log("Fail : load texture (In-Game/Tile/1_0)");
				throw new System.IO.FileNotFoundException("Failed to load texture (Dummy/1)");
			}
		}

        public static Vector2 Size { get { return size; } }
		
		public static Vector2 VectorPosition (this Index idx, Vector2 pivot) {
			return new Vector2(
				pivot.x + size.x * idx.col + (idx.row % 2) * (size.x / 2),
				-(pivot.y + size.y * idx.row / 2) );
		}
		
		public static Index IndexPosition (this Vector2 vec, Vector2 pivot) {
			// 45도 회전해서 타일 위치 구한 후 다시 -45도 회전해서 최종 위치 계산(...
			Vector2 relpos = new Vector2((vec.x - pivot.x) / size.x, -(vec.y + pivot.y) / size.y);
			Vector2 rotpos = new Vector2(relpos.x + relpos.y, relpos.x - relpos.y);
			Index rotidx = new Index(Mathf.RoundToInt(rotpos.x), Mathf.RoundToInt(rotpos.y));
			return rotidx.UnRotated;
		}
	}
	
}