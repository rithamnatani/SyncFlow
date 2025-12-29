import azure.functions as func
import datetime
import json
import logging

app = func.FunctionApp()

@app.service_bus_queue_trigger(
    arg_name="azmsg", 
    queue_name="orders", 
    connection="ServiceBusConnection"
)
@app.generic_output_binding(arg_name="signalr", type="signalR", hubName="chat", connectionStringSetting="AzureSignalRConnectionString")
def worker_function(azmsg: func.ServiceBusMessage, signalr: func.Out[str]):
    message_body = azmsg.get_body().decode('utf-8')
    logging.info('Python Service Bus queue trigger processed message: %s', message_body)
    
    # Broadcast the message to the 'chat' hub
    # In a real app, you might want to send to a specific user or group
    # output format: [{ "target": "TargetMethod", "arguments": [ "Arg1", "Arg2" ] }]
    signalr.set(json.dumps([{
        "target": "newMessage",
        "arguments": [message_body]
    }]))
