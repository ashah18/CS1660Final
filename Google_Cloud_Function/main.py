from functions_framework import http
import requests
import json
import os

def preprocess_file(data):
    # Implement your preprocessing logic here
    return data

@http
def handle_request(request):
    """Background Cloud Function to be triggered by an event.
    Args:
        cloud_event (cloudevents.http.event.CloudEvent): The CloudEvent object.
    """
    data = request.get_json()

    preprocessed_data = preprocess_file(data)
    
    prediction_endpoint = os.environ['PREDICTION_ENDPOINT']
    response = requests.post(prediction_endpoint, json=preprocessed_data)

    print(f"Received http prediction request with ID: {request} and data {request.get_json()}")

    return response.json()