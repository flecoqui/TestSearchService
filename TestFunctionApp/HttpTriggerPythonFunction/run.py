import os
import json
import urlparse

_AZURE_FUNCTION_DEFAULT_METHOD = "GET"
_AZURE_FUNCTION_HTTP_INPUT_ENV_NAME = "req"
_AZURE_FUNCTION_HTTP_OUTPUT_ENV_NAME = "res"
_REQ_PREFIX = "REQ_"

def write_http_response(status, body_dict):
    return_dict = {
        "status": status,
        "body": json.dumps(body_dict),
        "headers": {
            "Content-Type": "application/json"
        }
    }
    output = open(os.environ[_AZURE_FUNCTION_HTTP_OUTPUT_ENV_NAME], 'w')
    output.write(json.dumps(return_dict))

env = os.environ

# Get HTTP METHOD
http_method = env['REQ_METHOD'] if 'REQ_METHOD' in env else _AZURE_FUNCTION_DEFAULT_METHOD
print("HTTP METHOD => {}".format(http_method))

# Get QUERY STRING
req_url = env['REQ_HEADERS_X-ORIGINAL-URL'] if 'REQ_HEADERS_X-ORIGINAL-URL' in env else ''
urlparts =req_url.split('?') 
query_string = urlparts[1] if len(urlparts) == 2 else ''
print("QUERY STRING => {}".format(query_string))
params = urlparse.parse_qs(query_string)

res_body = {}
req_body = {}
if http_method.lower() == 'post':
    req_body = eval(open(env[_AZURE_FUNCTION_HTTP_INPUT_ENV_NAME], "r").read())
    print("REQUEST BODY => {}".format(req_body))

arrayparam1 = params.get('param1')
if not arrayparam1:
    param1 = req_body.get('param1')
else:
    param1 = arrayparam1[0]

if param1:
    arrayparam2 = params.get('param2')
    if not arrayparam2:
        param2 = req_body.get('param2')
    else:
        param2 = arrayparam2[0]

    if param2:
        res_body["Message"] = "Param1: {} Param2: {}".format(param1,param2)
        write_http_response(200, res_body)
    else:
        res_body["Error"] = "Please pass param2 on the query string or in the request body"
        write_http_response(400, res_body)
else:
    res_body["Error"] = "Please pass param1 on the query string or in the request body"
    write_http_response(400, res_body)

# res_body = {}
# print("Dump ENVIRONMENT VARIABLES:")
# for k in env:
#     print("ENV: {0} => {1}".format(k, env[k]))
#     if (k.startswith(_REQ_PREFIX)):
#         res_body[k] = env[k]

# write_http_response(200, res_body)
