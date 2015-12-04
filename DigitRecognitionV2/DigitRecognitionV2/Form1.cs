using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DigitRecognitionV2
{
    public partial class Form1 : Form
    {
        const string brainSaveLocation = "6.6.200_save1.txt";
        string trainingInfoSaveLocation = "";

        const int N_CONVLAYERS = 2; // 2 or more
        int[] N_FEATURE_MAPS = new int[N_CONVLAYERS + 1] { 1, 6, 6 };
        const int N_CLASSES = 10;
        const int N_HIDDEN_NEURONS = 200;
        const int WIDTH = 28; //divisible by 2^(N_CONVLAYERS)
        const int HEIGHT = 28; //divisible by 2^(N_CONVLAYERS)
        const int DRAW_WIDTH = 40;
        const int DRAW_HEIGHT = 50;
        const double INITIAL_LEARNING_RATE = 0.0001;
        const double INITIAL_HALF_RATE = 2;
        const int BATCH_SIZE = 3;
        const double HN_RETAIN_RATE = 0.5F;

        const int PIX_SIZE = 6;
        const int X_START = 4;
        const int Y_START = 4;
        const int BLACK_RAD = 4;
        const int BRUSH_RAD = 30;
        const float MIN_DARKNESS = 0.1F;
        double[,] drawBoard = new double[DRAW_HEIGHT, DRAW_WIDTH];

        double[,] input = new double[HEIGHT, WIDTH];
        double[, , ,][,] testCases;
        double[][,] trainingSets;
        int[] trainingLabels;

        //neuron output storage
        double[][][,] tanhOut = new double[N_CONVLAYERS][][,]; // [layer][map][y,x]
        double[][][,] poolOut = new double[N_CONVLAYERS][][,]; // [layer][map][y,x]
        double[] HLOut = new double[N_HIDDEN_NEURONS];
        double[] CNNOut = new double[N_CLASSES];

        //target output values of final output
        double[] target = new double[N_CLASSES];

        int iteration = 0;
        double learningRate;

        //weights
        double[][,][,] convWeights = new double[N_CONVLAYERS][,][,]; // [layer][mapSource, mapDestination][y,x] -> last two indicies between 0-2
        double[][] convBiases = new double[N_CONVLAYERS][]; //[layer][map]
        double[, , ,] HLWeights; // [map, yOfLastPoolingLayer, xOfLastPoolingLayer, hiddenNeuron]
        double[] HLBiases = new double[N_HIDDEN_NEURONS];
        double[,] finalWeights = new double[N_HIDDEN_NEURONS, N_CLASSES];
        double[] finalBiases = new double[N_CLASSES];

        double[][,][,] convWeightsUpdate = new double[N_CONVLAYERS][,][,]; // [layer][mapSource, mapDestination][y,x] -> last two indicies between 0-2
        double[][] convBiasesUpdate = new double[N_CONVLAYERS][]; //[layer][map]
        double[, , ,] HLWeightsUpdate; // [map, yOfLastPoolingLayer, xOfLastPoolingLayer, hiddenNeuron]
        double[] HLBiasesUpdate = new double[N_HIDDEN_NEURONS];
        double[,] finalWeightsUpdate = new double[N_HIDDEN_NEURONS, N_CLASSES];
        double[] finalBiasesUpdate = new double[N_CLASSES];

        //partial derivative storage
        double[] lastDer = new double[N_CLASSES];
        double[] hiddenDer = new double[N_HIDDEN_NEURONS];
        double[][][,] poolDer = new double[N_CONVLAYERS][][,];
        double[][][,] convDer = new double[N_CONVLAYERS][][,];
        bool realRun = false;

        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            InitializeArrays();
            ReadTests2();
            if (trainingInfoSaveLocation == "")
            {
                int counter = 1;
                for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
                {
                    trainingInfoSaveLocation += N_FEATURE_MAPS[convLayer + 1] + ".";
                }
                trainingInfoSaveLocation += N_HIDDEN_NEURONS + "." + INITIAL_LEARNING_RATE + "." + INITIAL_HALF_RATE + ".save";
                /*while (File.Exists(trainingInfoSaveLocation + counter + ".txt"))
                {
                    counter++;
                }*/
                trainingInfoSaveLocation += counter + ".txt";
            }
            //using (StreamWriter writer = new StreamWriter(trainingInfoSaveLocation))
            //    writer.WriteLine(DateTime.Now.Date);
        }

        void InitializeArrays()
        {
            for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
            {
                tanhOut[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]][,];
                poolOut[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]][,];
                convWeights[convLayer] = new double[N_FEATURE_MAPS[convLayer], N_FEATURE_MAPS[convLayer + 1]][,];
                convBiases[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]];
                convWeightsUpdate[convLayer] = new double[N_FEATURE_MAPS[convLayer], N_FEATURE_MAPS[convLayer + 1]][,];
                convBiasesUpdate[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]];
                poolDer[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]][,];
                convDer[convLayer] = new double[N_FEATURE_MAPS[convLayer + 1]][,];
                for (int map = 0; map < N_FEATURE_MAPS[convLayer + 1]; map++)
                {
                    tanhOut[convLayer][map] = new double[HEIGHT / (int)Math.Pow(2, convLayer), WIDTH / (int)Math.Pow(2, convLayer)];
                    poolOut[convLayer][map] = new double[HEIGHT / (int)Math.Pow(2, convLayer + 1), WIDTH / (int)Math.Pow(2, convLayer + 1)];
                    poolDer[convLayer][map] = new double[HEIGHT / (int)Math.Pow(2, convLayer + 1), WIDTH / (int)Math.Pow(2, convLayer + 1)];
                    convDer[convLayer][map] = new double[HEIGHT / (int)Math.Pow(2, convLayer), WIDTH / (int)Math.Pow(2, convLayer)];
                    for (int mapSource = 0; mapSource < N_FEATURE_MAPS[convLayer]; mapSource++)
                    {
                        convWeights[convLayer][mapSource, map] = new double[3, 3];
                        convWeightsUpdate[convLayer][mapSource, map] = new double[3, 3];
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                convWeights[convLayer][mapSource, map][i, j] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(9 * N_FEATURE_MAPS[convLayer]);
                            }
                        }
                    }
                    convBiases[convLayer][map] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(9 * N_FEATURE_MAPS[convLayer]);
                }
            }
            HLWeights = new double[N_FEATURE_MAPS[N_CONVLAYERS], HEIGHT / (int)Math.Pow(2, N_CONVLAYERS), WIDTH / (int)Math.Pow(2, N_CONVLAYERS), N_HIDDEN_NEURONS];
            HLWeightsUpdate = new double[N_FEATURE_MAPS[N_CONVLAYERS], HEIGHT / (int)Math.Pow(2, N_CONVLAYERS), WIDTH / (int)Math.Pow(2, N_CONVLAYERS), N_HIDDEN_NEURONS];
            for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
            {
                for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                {
                    for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                    {
                        for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                        {
                            HLWeights[map, y, x, hn] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(N_FEATURE_MAPS[N_CONVLAYERS] * HEIGHT / (int)Math.Pow(2, N_CONVLAYERS) * WIDTH / (int)Math.Pow(2, N_CONVLAYERS));

                        }
                    }
                }
                HLBiases[hn] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(N_FEATURE_MAPS[N_CONVLAYERS] * HEIGHT / (int)Math.Pow(2, N_CONVLAYERS) * WIDTH / (int)Math.Pow(2, N_CONVLAYERS));
            }
            for (int outClass = 0; outClass < N_CLASSES; outClass++)
            {
                for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                {
                    finalWeights[hn, outClass] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(N_HIDDEN_NEURONS);
                }
                finalBiases[outClass] = (rnd.NextDouble() * 2 - 1) / Math.Sqrt(N_HIDDEN_NEURONS);
            }
        }

        void ReadTests()
        {
            using (StreamReader reader = new StreamReader("testCases.txt"))
            {
                int totalTests = int.Parse(reader.ReadLine());
                double[][,] tests = new double[totalTests][,];

                testCases = new double[totalTests, 4, 4, N_CLASSES][,];

                for (int test = 0; test < totalTests; test++)
                {
                    for (int num = 0; num < 10; num++)
                    {
                        tests[test] = new double[16, 12];
                        for (int row = 0; row < 16; row++)
                        {
                            string[] line = reader.ReadLine().Split().ToArray();
                            for (int col = 0; col < 12; col++)
                            {
                                tests[test][row, col] = double.Parse(line[col]);
                            }
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                testCases[test, i, j, num] = new double[HEIGHT, WIDTH];
                                for (int y = 0; y < HEIGHT; y++)
                                    for (int x = 0; x < WIDTH; x++)
                                        testCases[test, i, j, num][y, x] = -1;
                                for (int k = 0; k < 16; k++)
                                {
                                    for (int l = 0; l < 12; l++)
                                    {
                                        testCases[test, i, j, num][k + i, l + j] = tests[test][k, l] * 2 - 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void ReadTests2()
        {
            using (BinaryReader bRead = new BinaryReader(File.Open("train-images.idx3-ubyte", FileMode.Open)))
            {
                bRead.ReadInt32();
                bRead.ReadInt32();
                bRead.ReadInt32();
                bRead.ReadInt32();
                trainingSets = new double[60000][,];
                for (int image = 0; image < 60000; image++)
                {
                    trainingSets[image] = new double[28, 28];
                    for (int row = 0; row < 28; row++)
                    {
                        for (int col = 0; col < 28; col++)
                        {
                            trainingSets[image][row, col] = bRead.ReadByte() / 255 * 2 - 1;
                        }
                    }
                }
            }
            using (BinaryReader bRead = new BinaryReader(File.Open("train-labels.idx1-ubyte", FileMode.Open)))
            {
                bRead.ReadInt32();
                bRead.ReadInt32();
                trainingLabels = new int[60000];
                for (int image = 0; image < 60000; image++)
                {
                    trainingLabels[image] = bRead.ReadByte();
                }
            }
        }

        void LoadWeights()
        {
            using (StreamReader reader = new StreamReader(brainSaveLocation))
            {
                iteration = int.Parse(reader.ReadLine());
                for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
                {
                    for (int map = 0; map < N_FEATURE_MAPS[convLayer + 1]; map++)
                    {
                        for (int sourceMap = 0; sourceMap < N_FEATURE_MAPS[convLayer]; sourceMap++)
                        {
                            string[] line = reader.ReadLine().Split().ToArray();
                            for (int ver = 0; ver < 3; ver++)
                            {
                                for (int hor = 0; hor < 3; hor++)
                                {
                                    convWeights[convLayer][sourceMap, map][ver, hor] = double.Parse(line[ver * 3 + hor]);
                                }
                            }
                        }
                        convBiases[convLayer][map] = double.Parse(reader.ReadLine());
                    }
                    reader.ReadLine();
                }
                reader.ReadLine();
                for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                {
                    for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                    {
                        string[] line = reader.ReadLine().Split().ToArray();
                        for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                        {
                            for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                            {
                                HLWeights[map, y, x, hn] = double.Parse(line[y * (WIDTH / (int)Math.Pow(2, N_CONVLAYERS)) + x]);
                            }
                        }
                    }
                    HLBiases[hn] = double.Parse(reader.ReadLine());
                }
                reader.ReadLine();
                for (int outClass = 0; outClass < N_CLASSES; outClass++)
                {
                    string[] line = reader.ReadLine().Split().ToArray();
                    for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                    {
                        finalWeights[hn, outClass] = double.Parse(line[hn]);
                    }
                    finalBiases[outClass] = double.Parse(reader.ReadLine());
                }
            }
        }

        void RunNeuralNet()
        {
            double[][,] currentOut = new double[1][,];
            currentOut[0] = input;

            //convolution & pooling stages
            for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
            {
                Parallel.For(0, N_FEATURE_MAPS[convLayer + 1], map =>
                {
                    //convolution
                    for (int y = 0; y < HEIGHT / (int)Math.Pow(2, convLayer); y++)
                    {
                        for (int x = 0; x < WIDTH / (int)Math.Pow(2, convLayer); x++)
                        {
                            double sum = 0;
                            for (int ver = -1; ver < 2; ver++)
                            {
                                for (int hor = -1; hor < 2; hor++)
                                {
                                    if (y + ver >= 0 && y + ver < currentOut[0].GetLength(0) && x + ver >= 0 && x + ver < currentOut[0].GetLength(1))
                                    {
                                        for (int sourceMap = 0; sourceMap < currentOut.Length; sourceMap++)
                                        {
                                            sum += currentOut[sourceMap][y + ver, x + ver] * convWeights[convLayer][sourceMap, map][ver + 1, ver + 1];
                                        }
                                    }
                                }
                            }
                            sum += convBiases[convLayer][map];
                            tanhOut[convLayer][map][y, x] = Math.Tanh(sum);
                        }
                    }

                    //max pooling
                    for (int y = 0; y < HEIGHT / (int)Math.Pow(2, convLayer + 1); y++)
                    {
                        for (int x = 0; x < WIDTH / (int)Math.Pow(2, convLayer + 1); x++)
                        {
                            poolOut[convLayer][map][y, x] = MaxOf(tanhOut[convLayer][map][y * 2, x * 2], tanhOut[convLayer][map][y * 2, x * 2 + 1], tanhOut[convLayer][map][y * 2 + 1, x * 2], tanhOut[convLayer][map][y * 2 + 1, x * 2 + 1]);
                        }
                    }
                });
                currentOut = poolOut[convLayer];
            }

            //fully connected stage
            //hidden layer
            for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
            {
                if (rnd.NextDouble() < HN_RETAIN_RATE || realRun)
                {
                    double sum = 0;
                    for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                    {
                        for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                        {
                            for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                            {
                                sum += poolOut[N_CONVLAYERS - 1][map][y, x] * HLWeights[map, y, x, hn];
                            }
                        }
                    }
                    sum += HLBiases[hn];
                    HLOut[hn] = Math.Tanh(sum);
                }
                else
                    HLOut[hn] = 0;
            }
            //output layer
            for (int outClass = 0; outClass < N_CLASSES; outClass++)
            {
                double sum = 0;
                for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                {
                    sum += HLOut[hn] * finalWeights[hn, outClass];
                }
                sum += finalBiases[outClass];
                if (!realRun)
                    sum /= HN_RETAIN_RATE;
                CNNOut[outClass] = Math.Tanh(sum);
            }
        }

        void BackPropagation()
        {
            // HIDDEN LAYER - OUTPUT LAYER
            for (int outClass = 0; outClass < N_CLASSES; outClass++)
            {
                lastDer[outClass] = (target[outClass] - CNNOut[outClass]) * (1 - CNNOut[outClass] * CNNOut[outClass]); //der. of error in terms of final output * final output in terms of sum
                Parallel.For(0, N_HIDDEN_NEURONS, hn =>
                {
                    finalWeightsUpdate[hn, outClass] += learningRate * lastDer[outClass] * HLOut[hn]; //der. of error in terms of sum of last layer * sum of last layer in terms of weight
                });
                finalBiasesUpdate[outClass] += learningRate * lastDer[outClass];
            }

            // LAST POOLING LAYER - HIDDEN LAYER
            Parallel.For(0, N_HIDDEN_NEURONS, hn =>
            {
                if (HLOut[hn] != 0)
                {
                    double derSum = 0; //der. of error in terms of the output of the hidden neuron
                    for (int outClass = 0; outClass < N_CLASSES; outClass++)
                    {
                        derSum += lastDer[outClass] * finalWeights[hn, outClass]; //add all the derivatives of the branches where the output is forward propagated to
                    }
                    hiddenDer[hn] = derSum * (1 - HLOut[hn] * HLOut[hn]);
                }
                else
                    hiddenDer[hn] = 0;

                for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                {
                    for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                    {
                        for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                        {
                            HLWeightsUpdate[map, y, x, hn] += learningRate * hiddenDer[hn] * poolOut[N_CONVLAYERS - 1][map][y, x];
                        }
                    }
                }
                HLBiasesUpdate[hn] += learningRate * hiddenDer[hn];
            });

            //last conv layer
            Parallel.For(0, N_FEATURE_MAPS[N_CONVLAYERS], map =>
            {
                //pooling layer
                for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                {
                    for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                    {
                        double derSum = 0;
                        for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                        {
                            derSum += hiddenDer[hn] * HLWeights[map, y, x, hn];
                        }
                        poolDer[N_CONVLAYERS - 1][map][y, x] = derSum;

                        //convolution layer
                        for (int yConv = y * 2; yConv < y * 2 + 2; yConv++)
                        {
                            for (int xConv = x * 2; xConv < x * 2 + 2; xConv++)
                            {
                                if (tanhOut[N_CONVLAYERS - 1][map][yConv, xConv] == poolOut[N_CONVLAYERS - 1][map][y, x])
                                {
                                    convDer[N_CONVLAYERS - 1][map][yConv, xConv] = poolDer[N_CONVLAYERS - 1][map][y, x] * (1 - tanhOut[N_CONVLAYERS - 1][map][yConv, xConv] * tanhOut[N_CONVLAYERS - 1][map][yConv, xConv]);
                                    for (int sourceMap = 0; sourceMap < N_FEATURE_MAPS[N_CONVLAYERS - 1]; sourceMap++)
                                    {
                                        for (int ver = -1; ver < 2; ver++)
                                        {
                                            for (int hor = -1; hor < 2; hor++)
                                            {
                                                if (yConv + ver >= 0 && yConv + ver < poolOut[N_CONVLAYERS - 2][sourceMap].GetLength(0) && xConv + hor >= 0 && xConv + hor < poolOut[N_CONVLAYERS - 2][sourceMap].GetLength(1))
                                                {
                                                    convWeightsUpdate[N_CONVLAYERS - 1][sourceMap, map][ver + 1, hor + 1] += learningRate * convDer[N_CONVLAYERS - 1][map][yConv, xConv] * poolOut[N_CONVLAYERS - 2][sourceMap][yConv + ver, xConv + hor] / Math.Sqrt(WIDTH / (int)Math.Pow(2, N_CONVLAYERS) * HEIGHT / (int)Math.Pow(2, N_CONVLAYERS));
                                                }
                                            }
                                        }
                                    }
                                    convBiasesUpdate[N_CONVLAYERS - 1][map] += learningRate * convDer[N_CONVLAYERS - 1][map][yConv, xConv] / (WIDTH / (int)Math.Pow(2, N_CONVLAYERS + 1) * HEIGHT / (int)Math.Pow(2, N_CONVLAYERS + 1));
                                }
                                else
                                    convDer[N_CONVLAYERS - 1][map][yConv, xConv] = 0;
                            }
                        }
                    }
                }
            });

            //convolution weights
            for (int convLayer = N_CONVLAYERS - 2; convLayer >= 0; convLayer--)
            {
                //pooling
                Parallel.For(0, N_FEATURE_MAPS[convLayer + 1], map =>
                {
                    for (int y = 0; y < HEIGHT / Math.Pow(2, convLayer + 1); y++)
                    {
                        for (int x = 0; x < WIDTH / Math.Pow(2, convLayer + 1); x++)
                        {
                            double derSum = 0;
                            for (int destMap = 0; destMap < N_FEATURE_MAPS[convLayer + 2]; destMap++)
                            {
                                for (int ver = -1; ver < 2; ver++)
                                {
                                    for (int hor = -1; hor < 2; hor++)
                                    {
                                        if (y - ver >= 0 && y - ver < convDer[convLayer + 1][destMap].GetLength(0) && x - hor >= 0 && x - hor < convDer[convLayer + 1][destMap].GetLength(1))
                                        {
                                            derSum += convDer[convLayer + 1][destMap][y - ver, x - hor] * convWeights[convLayer + 1][map, destMap][ver + 1, hor + 1];
                                        }
                                    }
                                }
                            }
                            poolDer[convLayer][map][y, x] = derSum;


                            //convolution layer
                            for (int yConv = y * 2; yConv < y * 2 + 2; yConv++)
                            {
                                for (int xConv = x * 2; xConv < x * 2 + 2; xConv++)
                                {
                                    if (tanhOut[convLayer][map][yConv, xConv] == poolOut[convLayer][map][y, x])
                                    {
                                        convDer[convLayer][map][yConv, xConv] = poolDer[convLayer][map][y, x] * (1 - tanhOut[convLayer][map][yConv, xConv] * tanhOut[convLayer][map][yConv, xConv]);
                                        for (int sourceMap = 0; sourceMap < N_FEATURE_MAPS[convLayer]; sourceMap++)
                                        {
                                            for (int ver = -1; ver < 2; ver++)
                                            {
                                                for (int hor = -1; hor < 2; hor++)
                                                {
                                                    if (convLayer > 0)
                                                    {
                                                        if (yConv + ver >= 0 && yConv + ver < poolOut[N_CONVLAYERS - 2][sourceMap].GetLength(0) && xConv + hor >= 0 && xConv + hor < poolOut[N_CONVLAYERS - 2][sourceMap].GetLength(1))
                                                        {
                                                            convWeightsUpdate[convLayer][sourceMap, map][ver + 1, hor + 1] += learningRate * convDer[convLayer][map][yConv, xConv] * poolOut[N_CONVLAYERS - 2][sourceMap][yConv + ver, xConv + hor] / Math.Sqrt(WIDTH / (int)Math.Pow(2, convLayer + 1) * HEIGHT / (int)Math.Pow(2, convLayer + 1));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (yConv + ver >= 0 && yConv + ver < input.GetLength(0) && xConv + hor >= 0 && xConv + hor < input.GetLength(1))
                                                        {
                                                            convWeightsUpdate[convLayer][sourceMap, map][ver + 1, hor + 1] += learningRate * convDer[convLayer][map][yConv, xConv] * input[yConv + ver, xConv + hor] / Math.Sqrt(WIDTH * HEIGHT / 4);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        convBiasesUpdate[convLayer][map] += learningRate * convDer[convLayer][map][yConv, xConv] / (WIDTH * HEIGHT / 16);
                                    }
                                    else
                                        convDer[convLayer][map][yConv, xConv] = 0;
                                }
                            }
                        }
                    }
                });
            }
        }

        //double convWeightsTotal = 0;
        //double convWeightsUpTotal = 0;
        //double HNWeightsTotal = 0;
        //double HNWeightsUpTotal = 0;
        //double finalWeightsTotal = 0;
        //double finalWeightsUpTotal = 0;
        void UpdateWeights()
        {
            
            for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
            {
                for (int map = 0; map < N_FEATURE_MAPS[convLayer + 1]; map++)
                {
                    for (int mapSource = 0; mapSource < N_FEATURE_MAPS[convLayer]; mapSource++)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                //convWeightsTotal += Math.Abs(convWeights[convLayer][mapSource, map][i, j]);
                                //convWeightsUpTotal += Math.Abs(convWeightsUpdate[convLayer][mapSource, map][i, j] * 200 / N_HIDDEN_NEURONS);
                                convWeights[convLayer][mapSource, map][i, j] += convWeightsUpdate[convLayer][mapSource, map][i, j] * 200 / N_HIDDEN_NEURONS;
                                convWeightsUpdate[convLayer][mapSource, map][i, j] = 0;
                            }
                        }
                    }
                    convBiases[convLayer][map] += convBiasesUpdate[convLayer][map] * 1600 / N_HIDDEN_NEURONS;
                    convBiasesUpdate[convLayer][map] = 0;
                }
            }
            for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
            {
                for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                {
                    for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                    {
                        for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                        {
                            //HNWeightsTotal += Math.Abs(HLWeights[map, y, x, hn]);
                            //HNWeightsUpTotal += Math.Abs(HLWeightsUpdate[map, y, x, hn]);
                            HLWeights[map, y, x, hn] += HLWeightsUpdate[map, y, x, hn];
                            HLWeightsUpdate[map, y, x, hn] = 0;
                        }
                    }
                }
                HLBiases[hn] += HLBiasesUpdate[hn];
                HLBiasesUpdate[hn] = 0;
            }
            for (int outClass = 0; outClass < N_CLASSES; outClass++)
            {
                for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                {
                    //finalWeightsTotal += Math.Abs(finalWeights[hn, outClass]);
                    //finalWeightsUpTotal += Math.Abs(finalWeightsUpdate[hn, outClass] * 1.2);
                    finalWeights[hn, outClass] += finalWeightsUpdate[hn, outClass] * 1.2;
                    finalWeightsUpdate[hn, outClass] = 0;
                }
                finalBiases[outClass] += finalBiasesUpdate[outClass];
                finalBiasesUpdate[outClass] = 0;
            }

            //double r1 = finalWeightsUpTotal / finalWeightsTotal;
            //double r2 = HNWeightsUpTotal / HNWeightsTotal;
            //double r3 = convWeightsUpTotal / convWeightsTotal;
        }

        void UpdateTextboxes()
        {
            for (int i = 0; i < 10; i++)
                CNNOut[i]++;
            double[] ordered = CNNOut.OrderByDescending(x => x).ToArray();
            txtGuess.Text = Array.IndexOf(CNNOut, ordered[0]).ToString();
            txtConfidence.Text = ((int)(ordered[0] / CNNOut.Sum() * 100)).ToString();
            txtGuess2.Text = Array.IndexOf(CNNOut, ordered[1]).ToString();
            txtConfidence2.Text = ((int)(ordered[1] / CNNOut.Sum() * 100)).ToString();
            txtGuess3.Text = Array.IndexOf(CNNOut, ordered[2]).ToString();
            txtConfidence3.Text = ((int)(ordered[2] / CNNOut.Sum() * 100)).ToString();
        }

        void SaveWeights()
        {
            using (StreamWriter writer = new StreamWriter(brainSaveLocation))
            {
                writer.WriteLine(iteration);
                for (int convLayer = 0; convLayer < N_CONVLAYERS; convLayer++)
                {
                    for (int map = 0; map < N_FEATURE_MAPS[convLayer + 1]; map++)
                    {
                        for (int sourceMap = 0; sourceMap < N_FEATURE_MAPS[convLayer]; sourceMap++)
                        {
                            for (int ver = 0; ver < 3; ver++)
                            {
                                for (int hor = 0; hor < 3; hor++)
                                {
                                    writer.Write(convWeights[convLayer][sourceMap, map][ver, hor] + " ");
                                }
                            }
                            writer.WriteLine();
                        }
                        writer.WriteLine(convBiases[convLayer][map]);
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
                for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                {
                    for (int map = 0; map < N_FEATURE_MAPS[N_CONVLAYERS]; map++)
                    {
                        for (int y = 0; y < HEIGHT / (int)Math.Pow(2, N_CONVLAYERS); y++)
                        {
                            for (int x = 0; x < WIDTH / (int)Math.Pow(2, N_CONVLAYERS); x++)
                            {
                                writer.Write(HLWeights[map, y, x, hn] + " ");
                            }
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine(HLBiases[hn]);
                }
                writer.WriteLine();
                for (int outClass = 0; outClass < N_CLASSES; outClass++)
                {
                    for (int hn = 0; hn < N_HIDDEN_NEURONS; hn++)
                    {
                        writer.Write(finalWeights[hn, outClass] + " ");
                    }
                    writer.WriteLine();
                    writer.WriteLine(finalBiases[outClass]);
                }
            }
        }

        double MaxOf(double a, double b, double c, double d)
        {
            return Math.Max(Math.Max(a, b), Math.Max(c, d));
        }

        bool mouseDown = false;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            while (mouseDown)
            {
                int xMouseInBox = PointToClient(Cursor.Position).X - X_START - PIX_SIZE / 2;
                int yMouseInBox = PointToClient(Cursor.Position).Y - Y_START - PIX_SIZE / 2;

                int xLow = Math.Max(0, (xMouseInBox - BRUSH_RAD) / PIX_SIZE);
                int xHigh = Math.Min(DRAW_WIDTH, (xMouseInBox + BRUSH_RAD) / PIX_SIZE + 1);
                if (xLow <= xHigh)
                {
                    int yLow = Math.Max(0, (yMouseInBox - BRUSH_RAD) / PIX_SIZE);
                    int yHigh = Math.Min(DRAW_HEIGHT, (yMouseInBox + BRUSH_RAD) / PIX_SIZE + 1);
                    if (yLow <= yHigh)
                    {
                        for (int col = xLow; col < xHigh; col++)
                        {
                            for (int row = yLow; row < yHigh; row++)
                            {
                                double d = Math.Sqrt((col * PIX_SIZE - xMouseInBox) * (col * PIX_SIZE - xMouseInBox) + (row * PIX_SIZE - yMouseInBox) * (row * PIX_SIZE - yMouseInBox));
                                if (d <= BLACK_RAD)
                                {
                                    drawBoard[row, col] = 1;
                                }
                                else if (d <= BRUSH_RAD)
                                {
                                    drawBoard[row, col] = Math.Max((BRUSH_RAD - d) * (1 - MIN_DARKNESS) / (BRUSH_RAD - BLACK_RAD) + MIN_DARKNESS, drawBoard[row, col]);
                                }
                            }
                        }
                        Refresh();
                    }
                }

                Application.DoEvents();
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            drawBoard = new double[DRAW_HEIGHT, DRAW_WIDTH];
            Refresh();
        }

        private void btnGuess_Click(object sender, EventArgs e)
        {
            realRun = true;
            Scale();
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    input[y, x] = input[y, x] * 2 - 1;
                }
            }
            RunNeuralNet();
            UpdateTextboxes();
            Refresh();
        }

        private void btnIterate_Click(object sender, EventArgs e)
        {
            prgIteration.Maximum = 60000;
            int ptime = Environment.TickCount;
            int counter = 0;
            int correctCount = 0;
            int pCorrect = 0;
            learningRate = INITIAL_LEARNING_RATE * INITIAL_HALF_RATE / (INITIAL_HALF_RATE + iteration); ;
            for (; iteration < int.Parse(txtIteration.Text); iteration++)
            {
                ShuffleTests();
                double testError = 0;
                for (int i = 0; i < 60000 / BATCH_SIZE; i++)
                {
                    for (int j = 0; j < BATCH_SIZE; j++)
                    {
                        counter++;
                        realRun = false;
                        input = trainingSets[i * BATCH_SIZE + j];
                        target = new double[N_CLASSES] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                        target[trainingLabels[i * BATCH_SIZE + j]] = 1;

                        RunNeuralNet();
                        BackPropagation();

                        if (CNNOut[trainingLabels[i * BATCH_SIZE + j]] == CNNOut.Max())
                            correctCount++;

                        for (int outClass = 0; outClass < N_CLASSES; outClass++)
                            testError += (target[outClass] - CNNOut[outClass]) * (target[outClass] - CNNOut[outClass]);

                        if (Environment.TickCount > ptime + 1000)
                        {
                            ptime = Environment.TickCount;
                            this.Text = counter + " tests/s | " + iteration + " of " + txtIteration.Text + " previous correct: " + pCorrect;
                            counter = 0;
                            prgIteration.Value = i * BATCH_SIZE + j;
                        }
                        Application.DoEvents();
                        if (!this.Visible)
                            break;
                    }
                    UpdateWeights();
                }

                pCorrect = correctCount;
                correctCount = 0;

                using (StreamWriter writer = new StreamWriter(trainingInfoSaveLocation, true))
                    writer.WriteLine(iteration + " " + pCorrect);

                learningRate = INITIAL_LEARNING_RATE * INITIAL_HALF_RATE / (INITIAL_HALF_RATE + iteration);

                this.Text = counter + " tests/s | " + iteration + " of " + txtIteration.Text + " previous correct: " + pCorrect;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Text = "saving...";
            SaveWeights();
            this.Text = "saved";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            for (int row = 0; row < DRAW_HEIGHT + 1; row++)
            {
                e.Graphics.DrawLine(Pens.Black, X_START, Y_START + PIX_SIZE * row, X_START + PIX_SIZE * DRAW_WIDTH, Y_START + PIX_SIZE * row);
            }
            for (int column = 0; column < DRAW_WIDTH + 1; column++)
            {
                e.Graphics.DrawLine(Pens.Black, X_START + PIX_SIZE * column, Y_START, X_START + PIX_SIZE * column, Y_START + PIX_SIZE * DRAW_HEIGHT);
            }
            for (int row = 0; row < DRAW_HEIGHT; row++)
                for (int col = 0; col < DRAW_WIDTH; col++)
                {
                    int darkness = 255 - (int)(drawBoard[row, col] * 255);
                    if (drawBoard[row, col] >= MIN_DARKNESS)
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(darkness, darkness, darkness)), X_START + PIX_SIZE * col + 1, Y_START + PIX_SIZE * row + 1, PIX_SIZE - 1, PIX_SIZE - 1);
                }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            this.Text = "loading...";
            LoadWeights();
            this.Text = "loaded";
        }

        void Scale()
        {
            int yMin = 9999;
            int yMax = 0;
            int xMin = 9999;
            int xMax = 0;
            for (int row = 0; row < DRAW_HEIGHT; row++)
            {
                for (int col = 0; col < DRAW_WIDTH; col++)
                {
                    if (drawBoard[row, col] > 0)
                    {
                        if (yMin == 9999)
                            yMin = row;
                        if (row > yMax)
                            yMax = row;
                        if (col < xMin)
                            xMin = col;
                        if (col > xMax)
                            xMax = col;
                    }
                }
            }
            xMax++;
            yMax++;

            int curWidth = xMax - xMin;
            int curHeight = yMax - yMin;

            double[,] cropped = new double[curHeight, curWidth];
            for (int row = yMin; row < yMax; row++)
            {
                for (int col = xMin; col < xMax; col++)
                {
                    cropped[row - yMin, col - xMin] = drawBoard[row, col];
                }
            }

            double scaleFactor = Math.Min((double)20 / curWidth, (double)20 / curHeight);
            double xDis = (20 - scaleFactor * curWidth) / 2;
            double yDis = (20 - scaleFactor * curHeight) / 2;

            double xCenter = 9.5;
            double yCenter = 9.5;
            double counter = 0;
            double[,] target = new double[20, 20];
            for (int row = 0; row < 20; row++)
            {
                for (int col = 0; col < 20; col++)
                {
                    if ((int)Math.Round((row - yDis) / scaleFactor) >= 0 && Math.Round((row - yDis) / scaleFactor) < cropped.GetLength(0) && (int)Math.Round((col - xDis) / scaleFactor) >= 0 && (int)Math.Round((col - xDis) / scaleFactor) < cropped.GetLength(1))
                    {
                        target[row, col] = cropped[(int)Math.Round((row - yDis) / scaleFactor), (int)Math.Round((col - xDis) / scaleFactor)];
                        if (counter == 0)
                        {
                            yCenter = col;
                            xCenter = row;
                        }
                        else
                        {
                            yCenter += (row - yCenter) * target[row, col] / (target[row, col] + counter);
                            xCenter += (col - xCenter) * target[row, col] / (target[row, col] + counter);
                        }
                        counter += target[row, col];
                    }
                    else
                        target[row, col] = 0;
                }
            }

            int yStart = (int)Math.Round(13.5 - yCenter);
            int xStart = (int)Math.Round(13.5 - xCenter);
            if (yStart < 0)
                yStart = 0;
            else if (yStart > 7)
                yStart = 7;
            if (xStart < 0)
                xStart = 0;
            else if (xStart > 7)
                xStart = 7;
            input = new double[28, 28];
            for (int row = yStart; row < yStart + 20; row++)
            {
                for (int col = xStart; col < xStart + 20; col++)
                {
                    input[row, col] = target[row - yStart, col - xStart];
                }
            }
        }

        void ShuffleTests()
        {
            double[][,] shuffled = new double[60000][,];
            int[] shuffledLabels = new int[60000];
            List<double[,]> original = trainingSets.ToList();
            List<int> originalLabels = trainingLabels.ToList();
            for (int i = 0; i < 60000; i++)
            {
                int randomIndex = rnd.Next(0, 60000 - i);
                shuffled[i] = original[randomIndex];
                shuffledLabels[i] = originalLabels[randomIndex];
            }
            trainingSets = shuffled;
            trainingLabels = shuffledLabels;
        }
    }
}
