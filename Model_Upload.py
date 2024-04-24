from google.cloud import storage, aiplatform, functions_v1, pubsub_v1
from google.cloud.functions_v1.services.cloud_functions_service import CloudFunctionsServiceClient
from google.cloud.functions_v1.types.functions import EventTrigger, CreateFunctionRequest, CloudFunction
from google.api_core.exceptions import NotFound
from fastapi import FastAPI, File, UploadFile, HTTPException, Form, Depends, Request
from fastapi.responses import JSONResponse
from pydantic import BaseModel, Field
from pathlib import Path
from google.protobuf import descriptor_pb2 as duration 
#from pyngrok import ngrok
import ngrok
import glob
import random
import os 
import requests
import zipfile
import shutil
from typing import Optional
# from flask import Flask, request

app = FastAPI()

database_prediction = []

ngrok_token = os.getenv("NGROK_TOKEN")
google_credentials = os.getenv("GOOGLE_APPLICATION_CREDENTIALS")

ngrok.set_auth_token("2dxvW6uOCVD9I4MuRCVK7FegqfK_3RWagoHG5dE4wWUKbAXk7")
os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = "cloudcomputing-411615-465bc8c55d44.json" #enter the path to your service account key

class ModelUploadRequest(BaseModel):
    project_id: str
    location: str 
    bucket_name: str = None

class Prediction(BaseModel):
    prediction: dict

def checker(data: str = Form(...)):
    try:
        return ModelUploadRequest.model_validate_json(data)
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

async def get_ngrok_public_url():
    # Assuming ngrok is properly imported and set up to be used asynchronously
    ngrok_tunnel = await ngrok.forward(8080)
    return ngrok_tunnel.url()

print(get_ngrok_public_url())

@app.post("/initialize_model_and_upload_model")
async def initialize_model(request_data: ModelUploadRequest = Depends(checker), file: UploadFile = File(...)):

    model_uploader = ModelUpload(
        request_data.project_id,
        request_data.location,
        request_data.bucket_name
    )

    if not file.filename.endswith('.zip'):
        raise HTTPException(status_code=400, detail="Only zip files are accepted.")

    temp_dir = Path("temp_files")
    temp_dir.mkdir(exist_ok=True)
    temp_file_path = temp_dir / file.filename

    # Save the uploaded file to a temporary directory
    with temp_file_path.open("wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    with zipfile.ZipFile(temp_file_path, 'r') as zip_ref:
        zip_ref.extractall(temp_dir)
    
    public_url_1 = await get_ngrok_public_url()

    try:
        for filename in os.listdir(temp_dir):
            if filename != file.filename:  # Exclude the original zip file
                print(f'{temp_dir}/{filename}/')

                print(" * ngrok tunnel \"{}\" -> \"http://127.0.0.1\"".format(public_url_1))
                model_uploader.upload_model(f'{temp_dir}/{filename}/', 'model')
                Model_Endpoint = model_uploader.deploy_model_to_vetex_ai_endpoint('cloud_project_prediction_model', 'model')
                endpoint_id = Model_Endpoint.split('/')[-1]
                #prediction_url = f"https://{request_data.location}-aiplatform.googleapis.com/v1/{endpoint_id}:predict"
                source_zip_path = shutil.make_archive('cloud_function', 'zip', 'Google_Cloud_Function/')
                model_uploader.upload_cloud_function('Preprocess_function', source_zip_path, endpoint_id, public_url_1)
            
        return JSONResponse(status_code=200, content={"message": "Model uploaded successfully", "nrgok_url": public_url_1,"bucket_name": model_uploader.bucket_name})
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if temp_file_path.exists():
            os.remove(temp_file_path)

@app.post("/predict_input")
def predict_input(file: UploadFile = File(...), bucket_name: str = Form(...), project_id: str = Form(...)):
    temp_dir = Path("temp_files")
    temp_dir.mkdir(exist_ok=True)
    temp_file_path = temp_dir / file.filename

    # Save the uploaded file to a temporary directory
    with temp_file_path.open("wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    storage_client = storage.Client()
    bucket = storage_client.bucket(bucket_name)
    blob = bucket.blob(file.filename)
    blob.upload_from_filename(temp_file_path)

    return JSONResponse(status_code=200, content={"message": "Input sent to Model for prediction successfull"})



@app.post("/receive_prediction")
async def receive_prediction(request: Request):
    data = await request.json()
    print(data['predictions'][0][0])

    database_prediction.append(data['predictions'][0][0])    

    return JSONResponse(status_code=200, content={"message": "Data received successfully", "data": data['predictions']})

@app.get("/get_predictions")
async def get_predictions():
    try:
        prediction = database_prediction[-1]
        print(database_prediction[-1])
    except IndexError:
        prediction = None
    return JSONResponse(status_code=200, content={"message": "Data received successfully", "data": prediction})

class ModelUpload:
    def __init__(self, project_id, location, bucket_name = None):
        self.project_id = project_id
        self.location = location
        self.storage_client = storage.Client()
        aiplatform.init(project = self.project_id,location = self.location)
        if(bucket_name):
            self.bucket_name = bucket_name
            self.create_bucket(self.bucket_name)
        else:
            self.bucket_name = 'cloud_project_model_storage' + ''.join([str(random.randint(1,9)) for _ in range(5)])
            self.create_bucket(self.bucket_name)
        

    def create_bucket(self,bucket_name):
        bucket = storage.Bucket(self.storage_client,bucket_name)
        bucket.location = self.location
        try: 
            self.storage_client.create_bucket(bucket)
            print(f"Created Bucket {self.bucket_name} in {self.location}")
        except NotFound:
            print(f"Bucket {self.bucket_name} already exists")

    def upload_model(self,local_model_path,bucket_path):  
        bucket = self.storage_client.bucket(self.bucket_name)
        blobs = list(bucket.list_blobs(prefix = bucket_path))
        if blobs:
            print(f'Model artifact already exists in {bucket_path}')
        else:
            rel_paths = glob.glob(local_model_path + '/**', recursive=True)
            for rel_path in rel_paths:
                remote_path = f'{bucket_path}/{"/".join(rel_path.split(os.sep)[2:])}'
                print(f"remote_path is {remote_path}")
                print(f"rel_path is {rel_path}")
                if os.path.isfile(rel_path):
                    blob = bucket.blob(remote_path)
                    blob.upload_from_filename(rel_path,timeout =300)
        
            print(f'The model has been uploaded in filename {bucket_path}')
            
    def deploy_model_to_vetex_ai_endpoint(self,model_display_name,model_artifact_uri):
        
        model = aiplatform.Model.upload(
            display_name = model_display_name,
            artifact_uri = f"gs://{self.bucket_name}/{model_artifact_uri.split('/')[0]}",
            serving_container_image_uri = "gcr.io/cloud-aiplatform/prediction/tf2-cpu.2-8:latest"
        )
        print(f"Model has been uploaded in vertext ai under the name of {model_display_name}")
        
        endpoint = aiplatform.Endpoint.create(display_name = "f{model_display_name}-endpoint")

        deployed_model = endpoint.deploy(
            model = model,
            deployed_model_display_name = model_display_name,
            machine_type = 'n1-standard-4',
            min_replica_count = 1,
            max_replica_count = 2
        )
        print(f"Model has been deployed to endpoint: {endpoint.resource_name}")
        
        return endpoint.resource_name




    def upload_zip_to_buket(self,souce_zip_path,trigger_bucket):
        bucket = self.storage_client.bucket(self.bucket_name)
        blob = bucket.blob(souce_zip_path)
        blob.upload_from_filename(souce_zip_path)
        print(f"Zip File{souce_zip_path} has been upladed to bucket {trigger_bucket}")
        return None

    def upload_cloud_function(self,function_name,source_zip,endpoint_id,ngrok_url,trigger_bucket = None,entry_point = 'handle_request'):
        if(not trigger_bucket):
            trigger_bucket = self.bucket_name
        self.upload_zip_to_buket(source_zip,trigger_bucket)

        client = CloudFunctionsServiceClient()
        parent = client.common_location_path(self.project_id, self.location)
        source_url = f"gs://{self.bucket_name}/{source_zip}" 

        cloud_function = {
            "name": f"projects/{self.project_id}/locations/{self.location}/functions/{function_name}",
            "entry_point": entry_point,
            "runtime": "python39",
            "source_archive_url": source_url,
            "event_trigger": {
                "event_type": "google.storage.object.finalize",
                "resource": f"projects/_/buckets/{trigger_bucket}"
            },
            "timeout": "540s",
            "available_memory_mb": 256,
            "environment_variables": {
                "PROJECT": "cloudcomputing-411615",
                "ENDPOINT_ID": endpoint_id,
                "ngrok_url": ngrok_url
            }
        }
        
       
        request = CreateFunctionRequest(
        location=parent,
        function=cloud_function,
        )

        operation = client.create_function(request=request)
        print("Deploying function, this may take a while...")
        response = operation.result()  
        print(f"Function {function_name} has been deployed.")
        print(f"Deployed function details: {response}")

    def create_pub_sub(self,topic_name,subscription_name, bucket_name):
        publisher = pubsub_v1.PublisherClient()
        topic_path = publisher.topic_path(self.project_id,topic_name)
        topic = publisher.create_topic(request={"name": topic_path})
        print(f"Topic {topic.name} created.")

        email = self.storage_client.get_service_account_email()
        print(email)
        policy = publisher.get_iam_policy(request={"resource": topic_path})
        policy.bindings.add(role="roles/pubsub.publisher", members=[email])

        subscriber = pubsub_v1.SubscriberClient()
        subscription_path = subscriber.subscription_path(self.project_id,subscription_name)
        subscription = subscriber.create_subscription(request={"name": subscription_path, "topic": topic_path})
        print(f"Subscription {subscription.name} created.")
        
        self.create_bucket(bucket_name)
        print(f"Bucket {bucket_name} created.")

        #notification 
        bucket = self.storage_client.bucket(bucket_name)
        notification = bucket.notification(
            topic_name=topic_name,
            event_types=['OBJECT_FINALIZE'],  # Trigger on new object creation
            payload_format='JSON_API_V1'
        )
        notification.create()
        print(f"Notification for bucket {bucket_name} created.")
        return topic,subscription


    #def pubsub_cloud_function(self, ) #function will create a cloud function that will get triggered whenever we upload a image to the bucket 


# @app.route('/', methods=['POST'])
# def receive_prediction():
#     # Get the JSON data from the request
#     data = request.get_json()

#     # Do something with the data (e.g., print it)
#     print(data)

#     return "OK", 200

# project_id = 'cloudcomputing-411615'
# location = 'us-central1'
#bucket_name = 'model_storage_2212'

# local_model_path = 'Model/'
# bucket_model_path = 'model' # The path in the bucket where the model will be stored

# model_display_name = 'cloud_project_prediction_model'

# grok = ngrok.forward(5000)

# ModelUploader = ModelUpload(project_id,location)
# ModelUploader.upload_model(local_model_path,bucket_model_path)
# Model_Endpoint = ModelUploader.deploy_model_to_vetex_ai_endpoint(model_display_name,bucket_model_path)
# endpoint_id = Model_Endpoint.split('/')[-1]
# prediction_url =  f"https://{location}-aiplatform.googleapis.com/v1/{endpoint_id}:predict"

#make a function to make a pub/sub topic and whenever we upload a image then this will go the the topic and the cloud function will thet the subscription
# image_upload_bucket_name = "image_upload_storage"
# topic,subcription = ModelUploader.create_pub_sub('image_upload_topic','image_upload_subscription',image_upload_bucket_name)

# ModelUploader.create_cloud_function('image_upload_function','image_upload_subscription')
# ModelUploader.create_trigger('image_upload_function','image_upload_topic')

#we will have a function that uplaods the cloud function, the cloud function is a subscrber and takes the message and sends it to the endpoint, and puts it in the bucket

# print(" * ngrok tunnel \"{}\" -> \"http://127.0.0.1\"".format(grok.url()))
# source_zip_path = shutil.make_archive('cloud_function','zip','Google_Cloud_Function/')
# ModelUploader.upload_cloud_function('Preprocess_function',source_zip_path,endpoint_id,grok.url())
# app.run(port=5000)


#Upload Model
#Deploy Model -> get endpoint 
#upload cloud function 