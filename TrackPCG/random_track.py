import matplotlib.pyplot as plt
from scipy.spatial import ConvexHull
import numpy as np
import math
import json
import time

def points_distance_squared(a, b):
    return (a[0] - b[0]) * (a[0] - b[0]) + (a[1] - b[1]) * (a[1] - b[1])

def push_apart(in_points, dst = 4):
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

def add_rand_points(inp_point, diff = 1, max_disp = 2):
    rSet = np.zeros((2 * len(inp_point), 2))

    for i, point in enumerate(inp_point):
        rX = (np.random.uniform() ** diff) * max_disp
        rY = (np.random.uniform() ** diff) * max_disp

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

        rSet[i] = point * scale_factor

    return rSet

def save_to_json(outer, inner, checkpoints, checkpoints_angle, checkpoints_length, starts, starts_angle):
    list_outer = []
    list_inner = []
    list_checkpoints = []
    list_starts = []

    for point in outer:
        list_outer.append({
            "x": round(point[0], 4),
            "y": round(point[1], 4)
        })

    for point in inner:
        list_inner.append({
            "x": round(point[0], 4),
            "y": round(point[1], 4)
        })

    for i, point in enumerate(checkpoints):
        list_checkpoints.append({
            "order": i,
            "point": {
                "x": round(point[0], 4),
                "y": round(point[1], 4),
            },
            "length": round(checkpoints_length[i], 4),
            "angle": round(checkpoints_angle[i], 4)
        })

    for point in starts:
        list_starts.append({
            "point": {
                "x": round(point[0], 4),
                "y": round(point[1], 4),
            },
            "angle": round(starts_angle, 4)
        })

    data = {
        "outer": list_outer,
        "inner": list_inner,
        "checkpoints": list_checkpoints,
        "starts": list_starts
    }

    filename = "track_" + str(time.time()) + ".json"
    print("Saving to " + filename)

    with open(filename, "w") as f:
        json.dump(data, f, indent=4)

if __name__ == "__main__":
    while True:
        rng = np.random.default_rng()
        xs = np.random.uniform(-30, 30, (40,1))
        ys = np.random.uniform(-16, 16, (40,1))
        points = np.concatenate((xs, ys), axis=1)
        hull = ConvexHull(points)
        hull_points = np.array([points[i] for i in hull.vertices])

        for i in range(3):
            push_apart(hull_points, 5)

        outer_hull = add_rand_points(hull_points, 1, 5)

        # for i in range(3):
        #     push_apart(outer_hull, 5)

        inner_hull = scale(outer_hull, 0.55, 0.65)

        lastIndex = len(outer_hull) - 1

        checkpoints = np.zeros((len(outer_hull), 2))
        checkpoints_angle = np.zeros((len(outer_hull),))
        checkpoints_length = np.zeros((len(outer_hull),))

        for i in range(len(outer_hull)):
            checkpoints[i] = (outer_hull[i] + inner_hull[i]) / 2

            direction = outer_hull[i] - inner_hull[i]
            checkpoints_length[i] = math.sqrt(direction[0] * direction[0] + direction[1] * direction[1])
            normal = np.array([-direction[1], direction[0]])

            angle = math.degrees(math.atan2(normal[1], normal[0]))
            checkpoints_angle[i] = (angle + 360 + 180) % 360

        starts_points = np.zeros((3, 2))
        rand_index = rng.integers(0, lastIndex)
        next_index = (rand_index + 1) % len(outer_hull)
        starts_angle = math.degrees(math.atan2(outer_hull[next_index][1] - outer_hull[rand_index][1], outer_hull[next_index][0] - outer_hull[rand_index][0]))
        starts_angle = (starts_angle + 360 - 90) % 360
        outer_point = (outer_hull[rand_index] + outer_hull[next_index]) / 2
        inner_point = (inner_hull[rand_index] + inner_hull[next_index]) / 2

        for i in range(3):
            starts_points[i] = inner_point + (outer_point - inner_point) * (i + 1) / 4

        for i in range(len(outer_hull)):
            plt.plot([outer_hull[i][0], inner_hull[i][0]], [outer_hull[i][1], inner_hull[i][1]], 'g--')
            # plt.text(checkpoints[i][0] + 0.5, checkpoints[i][1] + 0.5, str(checkpoints_angle[i]))
            plt.text(checkpoints[i][0] - 1.5, checkpoints[i][1] + 0.5, str(i))

        plt.plot(checkpoints[:,0], checkpoints[:,1], 'go')
        plt.plot(starts_points[:,0], starts_points[:,1], "mo")
        plt.plot(outer_hull[:,0], outer_hull[:,1], 'r--', lw=2)
        plt.plot(outer_hull[::max(1,lastIndex),0], outer_hull[::max(1,lastIndex),1], 'r--', lw=2)
        plt.plot(inner_hull[:,0], inner_hull[:,1], 'b--', lw=2)
        plt.plot(inner_hull[::max(1,lastIndex),0], inner_hull[::max(1,lastIndex),1], 'b--', lw=2)

        plt.show(block=False)

        if input("Save? (y/n, default n)\n") == "y":
            save_to_json(outer_hull, inner_hull, checkpoints, checkpoints_angle, checkpoints_length, starts_points, starts_angle)

        if input("Generate again? (y/n, default n)\n") != "y":
            break
