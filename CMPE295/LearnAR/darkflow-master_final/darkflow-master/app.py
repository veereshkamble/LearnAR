from flask import Flask, jsonify, request, make_response
from subprocess import call
import circuitAnalysis
import json
import base64
import cktRecog
from PIL import Image

app = Flask(__name__)
lastImage = ''


@app.route('/')
def index():
    return "Hello World!"


@app.route('/circuit/image/labels', methods=['GET'])
def get_image_labels():
    with open("label_img/circuit_label.jpg", "rb") as image_file:
        encoded_string = base64.b64encode(image_file.read())

    image = {}
    # print(lastImage)
    # image['image'] = lastImage.decode('utf-8')
    image['image'] = encoded_string.decode('utf-8')

    return jsonify(image), 200
    return


@app.route('/circuit/image/hololens', methods=['GET'])
def get_image():
    circuitAnalysis.execute_get_labels()

    with open("sample_img/circuit.jpg", "rb") as image_file:
        encoded_string = base64.b64encode(image_file.read())

    image = {}
    # print(lastImage)
    # image['image'] = lastImage.decode('utf-8')
    image['image'] = encoded_string.decode('utf-8')

    return jsonify(image), 200


@app.route('/circuit/evaluate', methods=['GET'])
def evaluate():
    # circuitAnalysis.execute_model()

    width, height, dataLength, midPoint = cktRecog.getComponentMidPoint('sample_img/circuit.jpg', 'sample_img/out/circuit.json')
    permList = cktRecog.generateLines(width, height, dataLength, midPoint)
    intersect = cktRecog.getIntersection(permList)
    jsonObj = cktRecog.getVertices(intersect)

    return jsonify(jsonObj), 200


@app.route('/circuit/image/mobile', methods=['POST'])
def mobile_post():
    # circuitAnalysis.cleanup('sample_img/circuit.jpg', 'sample_img/out/circuit.json')

    image = request.json['image']
    image = image.replace("data:image/jpeg;base64,", "")
    print(image)

    with open('sample_img/circuit.jpg', 'wb') as fh:
        fh.write(base64.decodebytes(image.encode('utf-8')))

    #temp = Image.open('sample_img/circuit.jpg')
    # temp = temp.rotate(90, expand=True)
    #temp.save('sample_img/circuit.jpg')

    with open("sample_img/circuit.jpg", "rb") as image_file:
        encoded_string = base64.b64encode(image_file.read())

    global lastImage
    lastImage = encoded_string

    s = {}
    s['response'] = 'success'
    return jsonify(s), 201


@app.route('/circuit/image', methods=['GET', 'POST'])
def post_image():

    image = request.json['image']
    print(image)

    with open('sample_img/circuit.jpg', 'wb') as fh:
        fh.write(base64.decodebytes(image.encode('utf-8')))

    circuitAnalysis.execute_model()
    data = json.load(open('sample_img/out/circuit.json'))

    circuitAnalysis.cleanup('sample_img/circuit.jpg', 'sample_img/out/circuit.json')
    return jsonify(data), 201


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)