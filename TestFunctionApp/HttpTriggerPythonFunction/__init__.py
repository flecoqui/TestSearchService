import logging

import azure.functions as func


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    param1 = req.params.get('param1')
    if not param1:
        try:
            logging.info('Python HTTP trigger try')
            req_body = req.get_json()
        except ValueError:
            pass
        else:
            param1 = req_body.get('param1')

    if param1:
        param2 = req.params.get('param2')
        if not param2:
            try:
                logging.info('Python HTTP trigger try')
                req_body = req.get_json()
            except ValueError:
                pass
            else:
                param2 = req_body.get('param2')

        if param2:
            return func.HttpResponse(f"Param1: {param1} Param2: {param2}")
        else:
            return func.HttpResponse(
                "Please pass param2 on the query string or in the request body",
                status_code=400
            )
    else:
        return func.HttpResponse(
             "Please pass param1 on the query string or in the request body",
             status_code=400
        )
