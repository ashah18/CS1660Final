#End-to-End ML Prediction Pipeline using Google Cloud

This project involves uploading machine learning models to Google Cloud.

## Files

### Model_Upload.py

This file contains the `ModelUpload` class, which is used to upload models to Google Cloud. The class initializes with a `project_id`, `location`, and optionally a `bucket_name`. If no `bucket_name` is provided, a random one is generated.

The `ModelUpload` class uses the `google.cloud.storage` and `google.cloud.aiplatform` libraries to interact with Google Cloud.

## To Do

Based on the provided code, here are some things that might still need to be implemented:

- Error handling for the Google Cloud operations.
- Method to upload the preprocessing code from the user.
- Website for uploading model from user and receiving user prediction requests.
- Implement scalling techniques.
- Tests for the `ModelUpload` class.
