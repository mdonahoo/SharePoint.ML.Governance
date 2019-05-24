using System;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Runtime;


namespace SharePoint.ML.Governance
{
    class Program
    {
        static readonly string _trainDataPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Data", "traindata.csv");
        static readonly string _testDataPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Data", "testdata.csv");
        static readonly string _modelPath = Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Model", "GovernanceModel.zip");

        static void Main(string[] args)
        {

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine();


            Console.WriteLine("Loading ML....");
            MLContext mlContext = new MLContext(seed: 0);
            Console.WriteLine("Training....");
            var model = Train(mlContext, _trainDataPath);
            Console.WriteLine("Evaluating....");
            Evaluate(mlContext, model);
            Console.WriteLine("Predicting....");
            Predict(mlContext, string.Empty);
                
            // Potentially, the lines below can be in a different process altogether.
            // When you load the model, it's a non-specific ITransformer. We also recover
            // the original schema.


            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine();

        }

        public static ITransformer Train(MLContext mlContext, string dataPath)
        {
            IDataView trainingData = mlContext.Data.LoadFromTextFile<GovernanceData>(dataPath, hasHeader: true, separatorChar: ',');
            var preview = trainingData.Preview(10);

            var pipeline =
                 mlContext.Transforms.Concatenate("FeatureVector", "Age", "Security", "Activity", "Protection","Usage", "Change")
                 //.Append(mlContext.Transforms.NormalizeMinMax("FeatureVector"))
                 .Append(mlContext.Transforms.Categorical.OneHotEncoding("TenantOneHot", "Tenant"))
                 //.Append(mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "GoodBad", featureColumnName: "FeatureVector"));
                 .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "GoodBad", featureColumnName: "FeatureVector"));
            //.Append(mlContext.BinaryClassification.Trainers.Prior(labelColumnName: "Label", exampleWeightColumnName: null));
            //.Append(mlContext.BinaryClassification.Trainers.AveragedPerceptron());
            //.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "VendorIdEncoded", inputColumnName: "VendorId"))
            //.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "RateCodeEncoded", inputColumnName: "RateCode"))
            //.Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "PaymentTypeEncoded", inputColumnName: "PaymentType"))
            //.Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "", inputColumnName: ""));

            var model = pipeline.Fit(trainingData);

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model generated and trained              ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Saving Model.....");
            Console.WriteLine();

            // Saving and loading happens to transformers. We save the input schema with this model.
            mlContext.Model.Save(model, trainingData.Schema, _modelPath);

            return model;
        }

        private static void Evaluate(MLContext mlContext, ITransformer model)
        {
            IDataView testData = mlContext.Data.LoadFromTextFile<GovernancePrediction>(_testDataPath, hasHeader: true, separatorChar: ',');
            var predictions = model.Transform(testData);
            var metrics = mlContext.BinaryClassification.Evaluate(predictions, "GoodBad", "Score", "Probability", "PredictedGovernance");
            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model quality metrics evaluation         ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       Accuracy:      {metrics.Accuracy:0.##}");
            Console.WriteLine($"*       F1Score:      {metrics.F1Score:0.##}");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine();
        }

        private static void Predict(MLContext mlContext, string modelPath)
        {
            // Load Trained Model
            DataViewSchema predictionPipelineSchema;
            ITransformer predictionPipeline = mlContext.Model.Load(_modelPath, out predictionPipelineSchema);

            // Use the model for one-time prediction.
            // Make the prediction function object. Note that, on average, this call takes around 200x longer
            // than one prediction, so you might want to cache and reuse the prediction function, instead of
            // creating one per prediction.

            PredictionEngine<GovernanceData, GovernancePrediction> predictionEngine = mlContext.Model.CreatePredictionEngine<GovernanceData, GovernancePrediction>(predictionPipeline);
            // Obtain the prediction. Remember that 'Predict' is not reentrant. If you want to use multiple threads
            // for simultaneous prediction, make sure each thread is using its own PredictionEngine.

            // Input Data
            GovernanceData inputData = new GovernanceData
            {
                Age = 1F,
                Security = 1F,
                Activity = 1F,
                Protection = 1F,
                Usage = 1F,
                Change = 1F
            };
            // Actual data
            GovernanceData[] lotsOfData = new GovernanceData[]
            {
                new GovernanceData
                {
                    Age = 1F,
                    Security = 1F,
                    Activity = 1F,
                    Protection = 1F,
                    Usage = 1F,
                    Change = 1F
                },
                new GovernanceData
                {
                    Age = 0F,
                    Security = 0F,
                    Activity = 0F,
                    Protection = 0F,
                    Usage = 0F,
                    Change = 0F
                },
                new GovernanceData
                {
                    Age = 2F,
                    Security = 2F,
                    Activity = 2F,
                    Protection = 2F,
                    Usage = 2F,
                    Change = 2F
                }
            };
            IDataView predictiondata = mlContext.Data.LoadFromEnumerable<GovernanceData>(lotsOfData);
            // Get Prediction
            IDataView predictions = predictionPipeline.Transform(predictiondata);
            System.Collections.Generic.IEnumerable<float> scoreColumn = predictions.GetColumn<float>("Score");
            System.Collections.Generic.IEnumerable<float> probabilityColumn = predictions.GetColumn<float>("Probability");

            foreach(float score in scoreColumn)
            {
                Console.WriteLine();
                Console.WriteLine($"*************************************************");
                Console.WriteLine($"*       Model multiple prediction test           ");
                Console.WriteLine($"*------------------------------------------------");
                Console.WriteLine($"*       Score:      {score:0.##}");
                Console.WriteLine($"*------------------------------------------------");
                Console.WriteLine();
            }
            foreach (float prob in probabilityColumn)
            {
                Console.WriteLine();
                Console.WriteLine($"*************************************************");
                Console.WriteLine($"*       Model multiple prediction test           ");
                Console.WriteLine($"*------------------------------------------------");
                Console.WriteLine($"*       Probability:      {prob:0.##}");
                Console.WriteLine($"*------------------------------------------------");
                Console.WriteLine();
            }
            GovernancePrediction prediction = predictionEngine.Predict(inputData);
            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"*       Model single prediction test             ");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine($"*       Probability:      {prediction.Probability:0.##}");
            Console.WriteLine($"*       Score:      {prediction.Score:0.##}");
            Console.WriteLine($"*       PredictedLabel:      {prediction.PredictedGovernance:0.##}");
            Console.WriteLine($"*------------------------------------------------");
            Console.WriteLine();

        }
    }
}
