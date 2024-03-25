from google.cloud import storage, aiplatform
from google.protobuf import json_format
from google.protobuf.struct_pb2 import Value
from typing import Dict, List, Union
from PIL import Image
import numpy as np
import requests
import json
import os

def preprocess_file(data):
    # Implement your preprocessing logic here
    return data

def predict_custom_trained_model_sample(
    project: str,
    endpoint_id: str,
    instances: Union[Dict, List[Dict]],
    location: str = "us-central1",
    api_endpoint: str = "us-central1-aiplatform.googleapis.com",
):
    client_options = {"api_endpoint": api_endpoint}
    client = aiplatform.gapic.PredictionServiceClient(client_options=client_options)
    instances = instances if isinstance(instances, list) else [instances]
    instances = [
        json_format.ParseDict(instance_dict, Value()) for instance_dict in instances
    ]
    parameters_dict = {}
    parameters = json_format.ParseDict(parameters_dict, Value())
    endpoint = client.endpoint_path(
        project=project, location=location, endpoint=endpoint_id
    )
    response = client.predict(
        endpoint=endpoint, instances=instances
    )
    print("response")
    print(" deployed_model_id:", response.deployed_model_id)
    predictions = response.predictions
    for prediction in predictions:
        print(" prediction:", prediction)
    return response

def handle_request(event,context):
    storage_client = storage.Client()
    bucket = storage_client.bucket(event['bucket'])
    blob = bucket.blob(event['name'])

    if(blob.name.endswith('.json')):
        data = json.loads(blob.download_as_text())
    elif(blob.name.endswith('.txt')):
        data = blob.download_as_text()
    elif(blob.name.endswith('.jpg')):
        with open('/tmp/image.jpg','wb') as f:
            data = blob.download_to_file(f)
        data = Image.open('/tmp/image.jpg')
        data = data.resize((100,100),Image.Resampling.LANCZOS)
        data = np.asarray(data).astype(np.float32)
        data = data.reshape(1, 100,100,3)
        data = data.tolist()
    else:
        data = blob.download_as_bytes()
    preprocessed_data = preprocess_file(data)
    
    project = os.environ['PROJECT']
    endpoint_id = os.environ['ENDPOINT_ID']
    predictions = predict_custom_trained_model_sample(project, endpoint_id, preprocessed_data)
    prediction_json = json_format.MessageToDict(predictions._pb)
    ngrok_url = os.environ['ngrok_url']
    #send reponse to ngrok_url
    requests.post(ngrok_url, data = prediction_json) 
    
    return None