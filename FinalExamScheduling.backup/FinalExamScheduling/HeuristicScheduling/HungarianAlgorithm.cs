using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MnMatrix = MathNet.Numerics.LinearAlgebra.Double.Matrix;


namespace Alairas.Common
{
    public static class HungarianAlgorithm
    {
        private static bool AmendCopulation(MnMatrix graph, int[] copulationVerticesX, int[] copulationVerticesY, int lengthX, int lengthY, out int[,] vertices)
        {
            int[] verticesXLabel = new int[lengthX];
            int[] verticesYLabel = new int[lengthY];

            for (int i = 0; i < lengthX; i++)
            {
                if (copulationVerticesX[i] != -1) verticesXLabel[i] = -1;
                else verticesXLabel[i] = 0;
            }

            for (int i = 0; i < lengthY; i++)
            {
                verticesYLabel[i] = -1;
            }

            int foundedIndex = -1;
            int foundedDepth = -1;
            bool found = false;
            bool succes = true;
            for (int depth = 0; !found && succes; depth++)
            {
                if (depth % 2 == 0)
                {
                    succes = false;
                    for (int i = 0; i < lengthX; i++)
                    {
                        if (verticesXLabel[i] == depth)
                        {
                            for (int j = 0; j < lengthY; j++)
                            {
                                if (graph.At(i, j) > 0 && verticesYLabel[j] == -1)
                                {
                                    verticesYLabel[j] = depth + 1;
                                    succes = true;
                                    if (copulationVerticesY[j] == -1)
                                    {
                                        found = true;
                                        foundedIndex = j;
                                        foundedDepth = depth + 1;
                                        break;
                                    }
                                }
                            }
                        }
                        if (found) break;
                    }
                }
                else
                {
                    for (int i = 0; i < lengthY; i++)
                    {
                        if (verticesYLabel[i] == depth) verticesXLabel[copulationVerticesY[i]] = depth + 1;
                    }
                }
            }

            if (found)
            {
                while (foundedDepth > -1)
                {
                    // megkeresem az előző elemet a javítóútban 
                    int i = 0;
                    while (verticesXLabel[i] != foundedDepth - 1 || graph.At(i, foundedIndex) < 1) i++;

                    int temp = foundedIndex;
                    foundedIndex = copulationVerticesX[i];
                    copulationVerticesX[i] = temp;
                    copulationVerticesY[temp] = i;
                    if (foundedIndex != -1) copulationVerticesY[foundedIndex] = -1;
                    foundedDepth -= 2;
                }
                vertices = null;
                return true;
            }
            vertices = new int[2, lengthX];

            for (int i = 0; i < lengthX; i++)
            {
                if (verticesXLabel[i] == -1) vertices[0, i] = -1;
            }

            for (int i = 0; i < lengthY; i++)
            {
                if (verticesYLabel[i] == -1) vertices[1, i] = -1;
            }
            return false;
        }

        private static int CountCopulation(IEnumerable<int> copulation)
        {
            return copulation.Count(t => t == -1);
        }

        public static bool RunAlgorithm(MnMatrix graph, int[] copulationVerticesX, int[] copulationVerticesY, out int[,] vertices)
        {
            while (CountCopulation(copulationVerticesX) != 0)
            {
                bool success = AmendCopulation(graph, copulationVerticesX, copulationVerticesY, copulationVerticesX.Length, copulationVerticesY.Length, out vertices);
                if (!success)
                {
                    //                   Debug.WriteLine("Nincs több javítóút!");
                    return false;
                }
            }
            vertices = null;
            return true;
        }
    }
}
