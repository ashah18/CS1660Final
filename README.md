# End-to-End ML Prediction Pipeline using Google Cloud

This service is a FastAPI application that allows users to upload a model, input an image for prediction, and retrieve the latest prediction.

## Files

### Model_Upload.py

This file contains the `ModelUpload` class, which is used to upload models to Google Cloud. The class initializes with a `project_id`, `location`, and optionally a `bucket_name`. If no `bucket_name` is provided, a random one is generated.

The `ModelUpload` class uses the `google.cloud.storage` and `google.cloud.aiplatform` libraries to interact with Google Cloud.

It also contains the Fast API, which is hosted on Google Run. The Fast API contains the below endpoints:
- POST /upload_model: This endpoint is likely used to upload a model to Google Cloud. The details of the   request payload would depend on how the endpoint is implemented, but it might include the model file and possibly some metadata about the model.

-POST /predict_input: This endpoint is likely used to make a prediction using the uploaded model. The request payload would include the input data for the prediction.

-GET /get_predictions: This endpoint retrieves the latest prediction from the in-memory database. If no predictions have been received yet, it returns None.


### Google_Cloud_Function/main.py

This file contains the code for a Google Cloud Function that is deployed on Google Cloud. The function is triggered by a file upload in Google Cloud Storage bucket.

The function's code handles the preprocessing part and sends the preprocessed image to the vertex ai endpoint, where the model resides. It also recives the output from the model and sends it to the API using ngrok.

## Google Services Used

- **Google Cloud Storage**: Used to store the model files that are uploaded via the `/upload_model` endpoint. It also triggers the cloud function when a new file is uploaded.

- **Google Cloud Functions**: Runs serverless code in response to events in the Google Cloud environment. In this project, a function is triggered when a new file is uploaded to Google Cloud Storage. The function handles preprocessing of the image and sends the preprocessed image to the Vertex AI endpoint.

- **Google Vertex AI**: Hosts the machine learning model and makes predictions. Takes the preprocessed image from the Google Cloud Function as input and returns a prediction.

- **Google Cloud Run**: Hosts the FastAPI application. It automatically scales the application based on traffic and allows the application to be accessed via a public URL.