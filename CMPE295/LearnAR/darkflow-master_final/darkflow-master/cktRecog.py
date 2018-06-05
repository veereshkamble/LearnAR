from __future__ import division
import json
import itertools
from PIL import Image

# Get Top left and Bottom right coordinates (Bounded Box) of each component from JSON file
# def getBox():
#    return

# Calculate mid-point of each component
def getComponentMidPoint(image_file, json_file):
    midPoint = []
    img = Image.open(image_file)
    width, height = img.size

    # Data Length
    data = json.load(open(json_file))
    dataLength = len(data)

    for i in range(0, dataLength):
        tlX = data[i]['topleft']['x']
        tlY = data[i]['topleft']['y']
        brX = data[i]['bottomright']['x']
        brY = data[i]['bottomright']['y']
        midPoint.append([])

        midPoint[i].append(((brX - tlX) / 2) + tlX)
        midPoint[i].append(((brY - tlY) / 2) + tlY)

    print(midPoint)
    return width, height, dataLength, midPoint


def split(arr, size):
    arrs = []
    while len(arr) > size:
        pice = arr[:size]
        arrs.append(pice)
        arr = arr[size:]
    arrs.append(arr)
    return arrs


# Generate two lines per mid-point; One along X-axis and one along Y-axis
# Order is leftmost to rightmost for horizontal and topmost to bottommost for vertical line
def generateLines(width, height, dataLength, midPoint):
    lines = []
    for i in range(0, dataLength):
        lines.append(0)
        lines.append(midPoint[i][1])
        lines.append(width)
        lines.append(midPoint[i][1])
        lines.append(midPoint[i][0])
        lines.append(0)
        lines.append(midPoint[i][0])
        lines.append(height)
        # print(lines)

    lines1 = split(lines, 4)
    print(lines1)

    permList = list(itertools.permutations(lines1, 2))
    print(len(permList))
    # print(permList[0])
    # print(permList[1])
    # print(permList[0][0][0])
    return permList


# Find intersection
def line(p1, p2):
    A = (p1[1] - p2[1])
    B = (p2[0] - p1[0])
    C = (p1[0] * p2[1] - p2[0] * p1[1])
    return A, B, -C


def intersection(L1, L2):
    D = L1[0] * L2[1] - L1[1] * L2[0]
    Dx = L1[2] * L2[1] - L1[1] * L2[2]
    Dy = L1[0] * L2[2] - L1[2] * L2[0]
    if D != 0:
        x = Dx / D
        y = Dy / D
        return x, y
    else:
        return False


# Calculate intersection point of all generated lines
def getIntersection(permList):
    # print(permList)
    intersect = []
    for i in range(0, len(permList) - 1, 2):
        L1 = line([permList[i][0][0], permList[i][0][1]], [permList[i][0][2], permList[i][0][3]])
        L2 = line([permList[i][1][0], permList[i][1][1]], [permList[i][1][2], permList[i][1][3]])
        # print([permList[i][0][0], permList[i][0][1]])
        # print([permList[i][0][2], permList[i][0][3]])
        # print([permList[i+1][0][0], permList[i+1][0][1]])
        # print([permList[i+1][0][2], permList[i+1][0][3]])
        R = intersection(L1, L2)
        if R:
            intersect.append(R)
            # print("Intersection detected:", R)
        # else:
        # print("No single intersection point detected")
    print(intersect)
    return intersect


def getVertices(intersect):
    xMax = 0
    xMin = intersect[0][0]
    yMax = 0
    yMin = intersect[0][1]
    for i in range(0, len(intersect) - 1):
        # Max X
        if (intersect[i][0] > xMax):
            xMax = intersect[i][0]
        # Min X
        if (intersect[i][0] < xMin):
            xMin = intersect[i][0]
        # Max Y
        if (intersect[i][1] > yMax):
            yMax = intersect[i][1]
        # Min Y
        if (intersect[i][1] < yMin):
            yMin = intersect[i][1]
    print(xMax)
    print(xMin)
    print(yMax)
    print(yMin)

    obj = {}
    obj['xmin'] = xMin
    obj['xmax'] = xMax
    obj['ymin'] = yMin
    obj['ymax'] = yMax

    return obj

