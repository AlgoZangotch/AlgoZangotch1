using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Recorder.MFCC
{
    public class AudioSignal
    {
        public double[] data;
        public int sampleRate;
        public double signalLengthInMilliSec;
    }
    public class MFCCFrame
    {
        public double[] Features = new double[13];
    }
    public class SignalFrame
    {
        public double[] Data;
    }
    public class Sequence
    {
        public MFCCFrame[] Frames { get; set; }
    }
    static class MFCC
    {
        private static MATLABAudioFunctionsNative.AudioProcessing MATLABAudioObject = new MATLABAudioFunctionsNative.AudioProcessing();
        public static Sequence ExtractFeatures(double[] pSignal, int samplingRate)
        {
            Sequence sequence = new Sequence();
            double[,] mfcc = MATLABMFCCfunction(pSignal, samplingRate);
            int numOfFrames = mfcc.GetLength(1);
            int numOfCoefficients = mfcc.GetLength(0);
            Debug.Assert(numOfCoefficients == 13);
            sequence.Frames = new MFCCFrame[numOfFrames];
            for (int i = 0; i < numOfFrames; i++)
            {
                sequence.Frames[i] = new MFCCFrame();
                for (int j = 0; j < numOfCoefficients; j++)
                {
                    sequence.Frames[i].Features[j] = mfcc[j, i];
                }
            }
            return sequence;
        }
        public static SignalFrame[] DivideSignalToFrames(double[] pSignal, int pSamplingRate, double pSignalLengthInMilliSeconds, double pFrameLengthinMilliSeconds)
        {
            int numberOfFrames = (int)Math.Ceiling(pSignalLengthInMilliSeconds / pFrameLengthinMilliSeconds);
            //START FIX1
            int frameSize = (int)(pSamplingRate * pFrameLengthinMilliSeconds / 1000.0);
            //END FIX1
            //Start FIX2
            int remainingDataSize = pSignal.Length - frameSize * (numberOfFrames - 1);
            int compensation = (int)(remainingDataSize / frameSize);
            numberOfFrames += compensation;
            remainingDataSize -= compensation * frameSize;
            //End FIX2
            //initialize frames.
            SignalFrame[] frames = new SignalFrame[numberOfFrames];
            for (int i = 0; i < numberOfFrames; i++)
            {
                //START: FIX1
                frames[i] = new SignalFrame();
                //END: FIX1
                frames[i].Data = new double[frameSize];
            }
            //copy data from signal to frames.
            int signalIndex = 0;
            //START FIX1
            for (int i = 0; i < numberOfFrames - 1; i++)
            {
                Array.Copy(pSignal, signalIndex, frames[i].Data, 0, frameSize);
                signalIndex += frameSize;
            }
            Array.Copy(pSignal, signalIndex, frames[numberOfFrames - 1].Data, 0, remainingDataSize);
            //END FIX1 
            return frames;
        }
        //Voice Activation Detection (VAD)
        public static SignalFrame[] RemoveSilentSegments(SignalFrame[] pFrames)
        {
            double[] framesWeights = new double[pFrames.Length];
            int frameIndex = 0;
            foreach (SignalFrame frame in pFrames)
            {
                double squareMean = 0;
                double avgZeroCrossing = 0;
                for (int i = 0; i < frame.Data.Length - 1; i++)
                {
                    //FIX1
                    squareMean += frame.Data[i] * frame.Data[i];
                    // avgZeroCrossing += Math.Abs(Math.Sign(frame.Data[i+1]) - Math.Sign(frame.Data[i])) / 2;
                    avgZeroCrossing += Math.Abs(Math.Abs(frame.Data[i + 1]) - Math.Abs(frame.Data[i])) / 2.0;
                }
                squareMean /= frame.Data.Length;
                avgZeroCrossing /= frame.Data.Length;
                framesWeights[frameIndex++] = squareMean * (1 - avgZeroCrossing) * 1000;
            }
            double avgWeights = mean(framesWeights);
            double stdWeights = std(framesWeights);
            double gamma = 0.2 * Math.Pow(stdWeights, -0.8);
            double activationThreshold = avgWeights + gamma * stdWeights;

            //threshold weights.
            threshold(framesWeights, activationThreshold);
            //smooth weights to remove short silences.
            smooth(framesWeights);
            //set anything more than 0 with 1.
            threshold(framesWeights, 0);
            int numberOfActiveFrames = (int)framesWeights.Sum();
            SignalFrame[] activeFrames = new SignalFrame[numberOfActiveFrames];
            int activeFramesIndex = 0;
            for (int i = 0; i < pFrames.Length; i++)
            {
                if (framesWeights[i] == 1)
                {
                    activeFrames[activeFramesIndex] = new SignalFrame();
                    activeFrames[activeFramesIndex].Data = new double[pFrames[i].Data.Length];
                    pFrames[i].Data.CopyTo(activeFrames[activeFramesIndex].Data, 0);
                    activeFramesIndex++;
                }
            }
            return activeFrames;
        }

        public static double[] RemoveSilence(double[] pSignal, int pSamplingRate, double pSignalLengthInMilliSeconds, double pFrameLengthinMilliSeconds)
        {
            SignalFrame[] originalFrames = DivideSignalToFrames(pSignal, pSamplingRate, pSignalLengthInMilliSeconds, pFrameLengthinMilliSeconds);
            SignalFrame[] filteredFrames = RemoveSilentSegments(originalFrames);
            int signalLength = 0;
            foreach (SignalFrame frame in filteredFrames)
            {
                signalLength += frame.Data.Length;
            }
            double[] filteredSignal = new double[signalLength];
            int index = 0;
            foreach (SignalFrame frame in filteredFrames)
            {
                frame.Data.CopyTo(filteredSignal, index);
                index += frame.Data.Length;
            }
            return filteredSignal;
        }

        #region Private Methods.
        private static double mean(double[] arr)
        {
            return arr.Sum() / arr.Length;
        }
        private static double std(double[] arr)
        {
            double avg = mean(arr);
            double stdDev = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                stdDev += (arr[i] - avg) * (arr[i] - avg);
            }
            stdDev /= arr.Length;
            stdDev = Math.Sqrt(stdDev);
            return stdDev;
        }

        //smooth a signal with an averging filter with window size = 5;
        private static void smooth(double[] inputArr)
        {
            double[] arr = new double[inputArr.Length];
            inputArr.CopyTo(arr, 0);

            inputArr[1] = (arr[0] + arr[1] + arr[2]) / 3.0;
            for (int i = 2; i < arr.Length - 2; i++)
            {
                inputArr[i] = (arr[i - 2] + arr[i - 1] + arr[i] + arr[i + 1] + arr[i + 2]) / 5.0;
            }
            inputArr[arr.Length - 2] = (arr[arr.Length - 3] + arr[arr.Length - 2] + arr[arr.Length - 1]) / 3.0;
        }
        private static void threshold(double[] arr, double thr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > thr)
                {
                    arr[i] = 1;
                }
                else
                {
                    arr[i] = 0;
                }
            }
        }

        //private static MFCCFrame CalculateMFCC(SignalFrame signal,int samplingRate)
        //{           
        //    MFCCFrame res = new MFCCFrame();
        //    res.Features = MATLABMFCCfunction(signal.Data,samplingRate);
        //    return res;
        //}

        public static double[,] MATLABMFCCfunction(double[] signal, int samplingRate)
        {
            double[,] mfcc = (double[,])MATLABAudioObject.MATLABMFCCfunction(signal, (double)samplingRate);
            return mfcc;
        }
        #endregion

        /// <summary>
        /// Calculates the Euclidean distance between two frames.
        /// </summary>
        /// <param name="Feature1">The first Frame.</param>
        /// <param name="Feature2">The second Frame.</param>
        /// <returns>The Euclidean distance between the two Feature vectors.</returns>
        public static double EuclideanDistance(double[] Features1, double[] Features2)
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
        /// Runs the Dynamic Time Warping Algorithm
        /// </summary>
        /// <param name="templateSeq">Template Sequence</param>
        /// <param name="inputSeq">Input Sequence</param>
        /// <returns>the distance between the Template Sequence and the Input Sequence</returns>
        public static double DTWMatch(Sequence templateSeq, Sequence inputSeq)
        {
            // Optimized Version with O(N) Memory complexity
            MFCCFrame[] templateFrames = templateSeq.Frames, inputFrames = inputSeq.Frames;
            int N = templateFrames.Length, M = inputFrames.Length;
            const double Infinity = double.PositiveInfinity;
            double[] PrevCol = new double[N];

            // Initialize the last column (starting point)
            PrevCol[N - 1] = EuclideanDistance(templateFrames[N - 1].Features, inputFrames[M - 1].Features);
            PrevCol[N - 2] = EuclideanDistance(templateFrames[N - 2].Features, inputFrames[M - 1].Features);
            for (int templateIdx = 0; templateIdx + 2 < N; ++templateIdx)
                PrevCol[templateIdx] = Infinity;

            for (int inputIdx = M - 2; inputIdx >= 0; --inputIdx)
            {
                double[] CurrentCol = new double[N], currFeatures = inputFrames[inputIdx].Features;
                CurrentCol[N - 1] = EuclideanDistance(templateFrames[N - 1].Features, inputFrames[inputIdx].Features) + PrevCol[N - 1];

                double MinDistance = (PrevCol[N - 1] < PrevCol[N - 2] ? PrevCol[N - 1] : PrevCol[N - 2]);   // Min(PrevCol[N-1], PrevCol[N-2])
                CurrentCol[N - 2] = EuclideanDistance(templateFrames[N - 2].Features, inputFrames[inputIdx].Features) + MinDistance;

                for (int templateIdx = 0; templateIdx + 2 < N; ++templateIdx)
                {
                    MinDistance = PrevCol[templateIdx];   // Stretching

                    if (MinDistance > PrevCol[templateIdx + 1])   // One by One Matching
                        MinDistance = PrevCol[templateIdx + 1];

                    if (MinDistance > PrevCol[templateIdx + 2])   // Shrinking
                        MinDistance = PrevCol[templateIdx + 2];

                    CurrentCol[templateIdx] = EuclideanDistance(templateFrames[templateIdx].Features, currFeatures) + MinDistance;
                }
                PrevCol = CurrentCol;
            }
            return PrevCol[0];
        }

        public static Sequence GetNearestSequence(Sequence[] templateSequences, Sequence inputSequence)
        {
            double minDistance = double.MaxValue;
            Sequence nearestSequence = null;

            foreach (Sequence currSequence in templateSequences)
            {
                double distance = DTWMatch(currSequence, inputSequence);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestSequence = currSequence;
                }
            }
            return nearestSequence;
        }

        public static double beamSearch(Sequence templateSeq, Sequence inputSeq, double T)
        {

            int M = inputSeq.Frames.Length, N = templateSeq.Frames.Length;

            double[,] dp = new double[N + 1, M + 1];

            double Infinity = double.MaxValue;

            for (int tempInd = 0; tempInd < N; tempInd++)
                dp[tempInd, M] = Infinity;


            for (int inputInd = 0; inputInd <= M; inputInd++)
                dp[N, inputInd] = 0;

            M--;

            for (int tempInd = N - 1; tempInd >= 0; --tempInd)
            {
                double bCost = Infinity;

                for (int inputInd = M; inputInd >= 0; --inputInd)
                {
                    ref double dis = ref dp[tempInd, inputInd];

                    if (dp[inputInd + 1, tempInd] == Infinity)   //prevent overflow
                    {
                        dis = Infinity;
                    }
                    else
                        dis = dp[inputInd + 1, tempInd] + EuclideanDistance(inputSeq.Frames[inputInd].Features, templateSeq.Frames[tempInd].Features);    // Match the current input frame

                    double dis2 = dp[inputInd, tempInd + 1]; // skip curr template frame

                    if (dis > dis2)
                        dis = dis2;

                    if (dis < bCost)
                    {
                        bCost = dis;
                    }
                }

                for (int inputInd = M; inputInd >= 0; --inputInd)
                {
                    if (dp[tempInd, inputInd] > bCost + T)
                    {
                        dp[tempInd, inputInd] = Infinity;
                    }
                }
            }

            return dp[0, 0] == Infinity ? double.MaxValue : dp[0, 0];
        }


        public static Sequence asyncBeamSearch(Sequence[] templateSequences, Sequence inputSequence, double T)
        {

            int M = inputSequence.Frames.Length;
            int numTemplates = templateSequences.Length;
            int[] templateLengths = templateSequences.Select(seq => seq.Frames.Length).ToArray();

            // Initialize DP arrays per template
            double[][,] dpArrays = new double[numTemplates][,];

            for (int t = 0; t < numTemplates; t++)
            {
                int N = templateLengths[t];
                dpArrays[t] = new double[N + 1, M + 1];

                for (int tempInd = 0; tempInd < N; tempInd++)
                    dpArrays[t][tempInd, M] = double.MaxValue;

                for (int inputInd = 0; inputInd <= M; inputInd++)
                    dpArrays[t][N, inputInd] = 0;
            }

            M--;

            for (int tempInd = 0; tempInd < templateLengths.Max(); tempInd++)
            {
                // Global best cost across all templates for this frame
                double globalBest = double.MaxValue;

                // First, compute the new distances
                for (int t = 0; t < numTemplates; t++)
                {
                    if (tempInd >= templateLengths[t]) continue;
                    var dp = dpArrays[t];
                    var templateSeq = templateSequences[t];

                    for (int inputInd = M; inputInd >= 0; inputInd--)
                    {
                        if (dp[tempInd, inputInd] == double.MaxValue)
                        {
                            if (inputInd + 1 > M || dp[tempInd, inputInd + 1] == double.MaxValue)
                                dp[tempInd, inputInd] = double.MaxValue;
                            else
                                dp[tempInd, inputInd] = dp[tempInd + 1, inputInd] + EuclideanDistance(inputSequence.Frames[inputInd].Features, templateSeq.Frames[tempInd].Features);

                            double dis2 = dp[tempInd + 1, inputInd];
                            if (dp[tempInd, inputInd] > dis2)
                                dp[tempInd, inputInd] = dis2;
                        }

                        if (dp[tempInd, inputInd] < globalBest)
                            globalBest = dp[tempInd, inputInd];
                    }
                }

                // Then prune across all templates using globalBest + T
                for (int t = 0; t < numTemplates; t++)
                {
                    if (tempInd >= templateLengths[t]) continue;
                    var dp = dpArrays[t];

                    for (int inputInd = M; inputInd >= 0; inputInd--)
                    {
                        if (dp[tempInd, inputInd] > globalBest + T)
                            dp[tempInd, inputInd] = double.MaxValue;
                    }
                }
            }

            // Get best sequence by checking final DP values
            double bestScore = double.MaxValue;
            Sequence bestSequence = null;

            for (int t = 0; t < numTemplates; t++)
            {
                double score = dpArrays[t][0, 0];
                if (score < bestScore)
                {
                    bestScore = score;
                    bestSequence = templateSequences[t];
                }
            }

            return bestSequence;
        }

        public static int TimeSynchronousSearch(List<Sequence> templates, IEnumerable<MFCCFrame> inputStream,)
        {
            int T = templates.Count;
            const double Infinity = double.PositiveInfinity;

            // Current alignment score for each template
            double[] scores = new double[T];
            int[] positions = new int[T];  // position in each template
            for (int i = 0; i < T; i++) scores[i] = 0;

            foreach (var inputFrame in inputStream)
            {
                for (int t = 0; t < T; t++)
                {
                    Sequence template = templates[t];
                    int pos = positions[t];

                    if (pos >= template.Frames.Length)
                    {
                        continue; // this template has been fully matched
                    }

                    double cost = EuclideanDistance(template.Frames[pos].Features, inputFrame.Features);
                    scores[t] += cost;
                    positions[t]++;
                }
            }

            // Return index of best-matching template
            int bestTemplate = -1;
            double bestScore = Infinity;
            for (int t = 0; t < T; t++)
            {
                if (scores[t] < bestScore)
                {
                    bestScore = scores[t];
                    bestTemplate = t;
                }
            }

            return bestTemplate;
        }

    }


}
