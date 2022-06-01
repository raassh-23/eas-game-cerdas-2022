import matplotlib.pyplot as plt
from scipy.spatial import ConvexHull, convex_hull_plot_2d
import numpy as np
import math
import codecs, json
import time

def points_distance_squared(a, b):
    return (b[0] - a[0]) * (b[0] - a[0]) + (b[1] - a[1]) * (b[1] - a[1])

def push_apart(in_points):
    dst = 4
    dst_squared = dst * dst

    for i, point in enumerate(in_points):
        for j in range(i+1, len(in_points)):
            point_b = in_points[j]
            if points_distance_squared(point, point_b) < dst_squared:
                hx = point_b[0] - point[0]
                hy = point_b[1] - point[1]
                hl = math.sqrt(hx*hx + hy*hy)
                hx /= hl
                hy /= 1
                dif = dst - hl
                hx *= dif
                hy *= dif

                point[0] -= hx
                point_b[0] += hx
                point[1] -= hy
                point_b[1] += hy

def add_rand_points(inp_point):
    rSet = np.zeros((2 * len(inp_point), 2))
    diff = 1
    maxDisp = 2

    for i, point in enumerate(inp_point):
        rX = (np.random.uniform() ** diff) * maxDisp
        rY = (np.random.uniform() ** diff) * maxDisp

        next_point = inp_point[(i+1) % len(inp_point)]

        rSet[i*2] = point
        rSet[i*2 + 1][0] = (point[0] + next_point[0])/2 + rX
        rSet[i*2 + 1][1] = (point[1] + next_point[1])/2 + rY

    return rSet 

def fixAngle(inp_point, max_angle = 90): 
    for i, point in enumerate(inp_point):
        prev = inp_point[i-1 if i > 0 else (len(inp_point) - 1)]
        next = inp_point[(i+1) % len(inp_point)]

        px = point[0] - prev[0]
        py = point[1] - prev[1]
        pl = math.sqrt(px*px + py*py)
        px /= pl
        py /= pl

        nx = next[0] - point[0]
        ny = next[1] - point[1]
        nl = math.sqrt(nx*nx + ny*ny)
        nx /= nl
        ny /= nl

        a = math.atan2(px * ny - py * nx, px * nx + py * ny)
        if abs((a)) < max_angle:
            continue

        nA = math.radians(max_angle * math.copysign(1, a))
        diff = nA - a
        cos = math.cos(diff)
        sin = math.sin(diff)

        newX = nx * cos - ny * sin
        newY = nx * sin + ny * cos
        newX *= nl
        newY *= nl

        next[0] = point[0] + newX
        next[1] = point[1] + newX

def scale(inp_point, min_scale = 0.9, max_scale = None):
    if max_scale == None:
        max_scale = min_scale

    rSet = np.zeros((len(inp_point), 2))

    for i, point in enumerate(inp_point):
        scale_factor = np.random.uniform(min_scale, max_scale)

        next_point = inp_point[(i+1) % len(inp_point)]

        rSet[i] = point * scale_factor

    return rSet

def save_to_json(outer, inner):
    list_outer = []
    list_inner = []

    for point in outer:
        list_outer.append({"x": round(point[0], 4), "y": round(point[1], 4)})

    for point in inner:
        list_inner.append({"x": round(point[0], 4), "y": round(point[1], 4)})


    data = {
        "outer": list_outer,
        "inner": list_inner
    }

    filename = "track_" + str(time.time()) + ".json"
    print("Saving to " + filename)

    with open(filename, "w") as f:
        json.dump(data, f)

if __name__ == "__main__":
    rng = np.random.default_rng()
    xs = np.random.uniform(-30, 30, (40,1))
    ys = np.random.uniform(-18, 18, (40,1))
    points = np.concatenate((xs, ys), axis=1)
    hull = ConvexHull(points)
    hull_points = np.array([points[i] for i in hull.vertices])

    for i in range(5):
        push_apart(hull_points)

    new_hull_points = add_rand_points(hull_points) 

    inner_hull = scale(new_hull_points, 0.55, 0.65)

    lastIndex = len(new_hull_points) - 1

    plt.plot(new_hull_points[:,0], new_hull_points[:,1], 'r--', lw=2)
    plt.plot(new_hull_points[::max(1,lastIndex),0], new_hull_points[::max(1,lastIndex),1], 'r--', lw=2)
    plt.plot(inner_hull[:,0], inner_hull[:,1], 'b--', lw=2)
    plt.plot(inner_hull[::max(1,lastIndex),0], inner_hull[::max(1,lastIndex),1], 'b--', lw=2)

    plt.show()

    save = input("Save? (y/n)\n")

    if save == "y":
        save_to_json(new_hull_points, inner_hull)
