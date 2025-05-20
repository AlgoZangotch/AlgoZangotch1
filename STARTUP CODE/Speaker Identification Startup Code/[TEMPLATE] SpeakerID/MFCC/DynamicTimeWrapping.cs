using AForge.Math.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recorder.MFCC
{
    internal class DynamicTimeWrapping
    {
        /// <summary>
        /// Calculates the Euclidean distance between two frames.
        /// </summary>
        /// <param name="Feature1">The first Frame.</param>
        /// <param name="Feature2">The second Frame.</param>
        /// <returns>The Euclidean distance between the two Feature vectors.</returns>
        private static double EuclideanDistance(double[] Features1, double[] Features2)
        {
            double sum = 0;
            for (int i = 0; i < 13; i++)
            {
                double diff = Features1[i] - Features2[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Runs the Normal Dynamic Time Warping Algorithm
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double Match(Sequence templateSeq, Sequence inputSeq)
        {
            // Optimized Version with O(N) Memory complexity and O(N*M) Time complexity
            MFCCFrame[] templateFrames = templateSeq.Frames, inputFrames = inputSeq.Frames;
            int N = templateFrames.Length, M = inputFrames.Length;
            const double Infinity = double.PositiveInfinity;
            double[] currCol = new double[N], newCol = new double[N];

            // Initialize the last column (starting point)
            currCol[N - 1] = EuclideanDistance(templateFrames[N - 1].Features, inputFrames[M - 1].Features);
            currCol[N - 2] = EuclideanDistance(templateFrames[N - 2].Features, inputFrames[M - 1].Features);
            for (int templateIdx = 0; templateIdx + 2 < N; ++templateIdx)
                currCol[templateIdx] = Infinity;

            for (int inputIdx = M - 2; inputIdx >= 0; --inputIdx)
            {
                double[] currFeatures = inputFrames[inputIdx].Features;
                newCol[N - 1] = EuclideanDistance(templateFrames[N - 1].Features, inputFrames[inputIdx].Features) + currCol[N - 1];

                double minDistance = (currCol[N - 1] < currCol[N - 2] ? currCol[N - 1] : currCol[N - 2]);   // Min(currCol[N-1], currCol[N-2])
                newCol[N - 2] = EuclideanDistance(templateFrames[N - 2].Features, inputFrames[inputIdx].Features) + minDistance;

                for (int templateIdx = 0; templateIdx + 2 < N; ++templateIdx)
                {
                    minDistance = currCol[templateIdx];   // Stretching

                    if (minDistance > currCol[templateIdx + 1])   // One by One Matching
                        minDistance = currCol[templateIdx + 1];

                    if (minDistance > currCol[templateIdx + 2])   // Shrinking
                        minDistance = currCol[templateIdx + 2];

                    newCol[templateIdx] = EuclideanDistance(templateFrames[templateIdx].Features, currFeatures) + minDistance;
                }

                // Swapping
                double[] temp = newCol;
                newCol = currCol;
                currCol = temp;
            }
            return currCol[0];
        }

        /// <summary>
        /// Runs the Time-Synchronous Dynamic Time Warping Algorithm
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double TimeSyncMatch(Sequence templateSeq, Sequence inputSeq)
        {
            throw new NotImplementedException("TimeSyncMatch is not implemented yet.");
        }

        /// <summary>
        /// Runs Dynamic Time Warping Algorithm with Pruning by limiting Search Paths
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <param name="width">Width of the search path</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double LSPMatch(Sequence templateSeq, Sequence inputSeq, int width)
        {
            throw new NotImplementedException("LSPhMatch is not implemented yet.");
        }

        /// <summary>
        /// Runs Dynamic Time Warping Algorithm with Beam Search Pruning
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <param name="threshold">Threshold for pruning</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double BeamSearchMatch(Sequence templateSeq, Sequence inputSeq, double threshold)
        {
            throw new NotImplementedException("BeamSearchMatch is not implemented yet.");
        }

        /// <summary>
        /// Runs Time-Synchronous Dynamic Time Warping Algorithm with Beam Search Pruning
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <param name="threshold">Threshold for pruning</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double TimeSyncBeamSearchMatch(Sequence templateSeq, Sequence inputSeq, double threshold) // Optimized Version with O(N) Memory complexity
        {
            throw new NotImplementedException("TimeSyncBeamSearchMatch is not implemented yet.");
        }
    }
}
