using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MechEmpire
{
    public class AStarPathFinding : MonoBehaviour
    {
        private NetworkGamePlayerLobby player;

        public class data
        {
            public Vector2 position = new Vector2(0, 0);
            public bool blocked = false;
            public int gCost = 1;
            public int hCost = 1;
            public data parent;
            public List<data> adjacentNodes;
            public int fCost
            {
                get
                {
                    return gCost + hCost;
                }
            }

            public void setAdjacentNodes(data[,] nodes, NetworkGamePlayerLobby player)
            {

                adjacentNodes = new List<data>();
                int range = 1;

                int a = (int)position.x;
                int b = (int)position.y;

                for (int c = a - range; c <= a + range; c++)
                {
                    for (int d = b - range; d <= b + range; d++)
                    {
                        if ((c < player.mapSize && d < player.mapSize) && (c >= 0 && d >= 0))
                        {
                            data cNode = nodes[c, d];
                            adjacentNodes.Add(cNode);



                            int modifier = ((a % 2 == 0) ? -1 : 1);
                            int left = range;
                            int right = range;
                            for (int i = 0; i <= range; i++)
                            {
                                if (c == (a + i) || c == (a - i))
                                {
                                    if (Mathf.Abs(b - (d - ((range - right) * modifier))) > range)
                                    {
                                        adjacentNodes.Remove(cNode);
                                    }
                                    if (Mathf.Abs(b - (d + ((range - left) * modifier))) > range)
                                    {
                                        adjacentNodes.Remove(cNode);
                                    }
                                    if (cNode.position == position)
                                    {
                                        adjacentNodes.Remove(cNode);
                                    }
                                }
                                if (right == left)
                                {
                                    right--;
                                }
                                else
                                {
                                    left--;
                                }
                            }



                        }
                    }

                }




            }
        }
        public data[,] nodes;

        private List<data> finalPath;


        void Start()
        {
            player = gameObject.GetComponent<NetworkGamePlayerLobby>();
        }

        public List<Vector2> getFinalPath()
        {
            List<Vector2> path = new List<Vector2>();
            foreach (data node in finalPath)
            {
                path.Add(node.position);
            }
            return path;
        }

        public void setNodes()
        {
            nodes = new data[player.mapSize, player.mapSize];
            for (int _a = 0; _a < player.mapSize; _a++)
            {
                for (int _b = 0; _b < player.mapSize; _b++)
                {
                    nodes[_a, _b] = new data();
                    nodes[_a, _b].blocked = player.tiles[_a, _b].water;
                    nodes[_a, _b].position = new Vector2(_a, _b);
                }

            }
            for (int _a = 0; _a < player.mapSize; _a++)
            {
                for (int _b = 0; _b < player.mapSize; _b++)
                {
                    nodes[_a, _b].setAdjacentNodes(nodes, player);
                }

            }
        }

        int distanceBetweenNodes(Vector2 nodePos1, Vector2 nodePos2)
        {
            Vector2 difference = new Vector2(Mathf.Abs(nodePos1.x - nodePos2.x), Mathf.Abs(nodePos1.y - nodePos2.y));
            return ((int)(difference.x+difference.y) - Mathf.Min((int)difference.x,(int)difference.y));
        }

        public void getPath(Vector2 startPosition, Vector2 endPosition)
        {
            data startNode = nodes[(int)startPosition.x, (int)startPosition.y];
            data endNode = nodes[(int)endPosition.x, (int)endPosition.y];

            List<data> current = new List<data>();
            HashSet<data> visited = new HashSet<data>();
            current.Add(startNode);

            while (current.Count > 0)
            {
                data currentNode = current[0];
                for (int i = 0; i < current.Count; i++)
                {
                    if (current[i].fCost < currentNode.fCost ||
                    current[i].fCost == currentNode.fCost && current[i].hCost > currentNode.hCost)
                    {
                        currentNode = current[i];
                    }

                    visited.Add(currentNode);
                    current.Remove(currentNode);

                    if (currentNode.position == endNode.position)
                    {
                        List<data> path = new List<data>();
                        data _currentNode = endNode;
                        while (_currentNode != startNode)
                        {
                            path.Add(_currentNode);
                            _currentNode = _currentNode.parent;
                        }
                        path.Reverse();
                        finalPath = path;
                        return;
                    }

                    foreach (data adjNode in currentNode.adjacentNodes)
                    {
                        if (!(adjNode.blocked || visited.Contains(adjNode)))
                        {

                            int cost = currentNode.gCost + distanceBetweenNodes(currentNode.position, adjNode.position);
                            if (cost < adjNode.gCost || !current.Contains(adjNode))
                            {
                                adjNode.gCost = cost;
                                adjNode.hCost = distanceBetweenNodes(adjNode.position, endNode.position)*2;
                                adjNode.parent = currentNode;
                                if (!current.Contains(adjNode))
                                {
                                    current.Add(adjNode);
                                }
                            }
                        }

                    }

                }
            }



        }

    }
}

