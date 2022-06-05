import json
import matplotlib.pyplot as plt
import sys

filename = sys.argv[1]

with open(f"{filename}.json") as f:
    data = json.load(f)
    outer_hull_x = [p["x"] for p in data["outer"]]
    outer_hull_y = [p["y"] for p in data["outer"]]
    inner_hull_x = [p["x"] for p in data["inner"]]
    inner_hull_y = [p["y"] for p in data["inner"]]
    starts_x = [p["point"]["x"] for p in data["starts"]]
    starts_y = [p["point"]["y"] for p in data["starts"]]
    checkpoints_x = [p["point"]["x"] for p in data["checkpoints"]]
    checkpoints_y = [p["point"]["y"] for p in data["checkpoints"]]
    checkpoints_order = [p["order"] for p in data["checkpoints"]]
    lastIndex = len(outer_hull_x) - 1

    # for i in range(len(checkpoints_x)):
    #     plt.plot([outer_hull_y[i], inner_hull_x[i]], [outer_hull_y[i], inner_hull_y[i]], 'g--')
    #     plt.text(checkpoints_x[i] - 1.5, checkpoints_y[i] + 0.5, str(i))

    # plt.plot(checkpoints_x, checkpoints_y, 'go')
    # plt.plot(starts_x, starts_y, "mo")
    plt.plot(outer_hull_x, outer_hull_y, 'r--', lw=2)
    plt.plot(outer_hull_x[::max(1,lastIndex)], outer_hull_y[::max(1,lastIndex)], 'r--', lw=2)
    plt.plot(inner_hull_x, inner_hull_y, 'b--', lw=2)
    plt.plot(inner_hull_x[::max(1,lastIndex)], inner_hull_y[::max(1,lastIndex)], 'b--', lw=2)
    plt.show()