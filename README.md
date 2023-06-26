# Drone
![code coverage badge](https://raw.githubusercontent.com/leandro-benedet-garcia/Drone/gh-pages/code_coverage/badge_linecoverage.svg)

This is a mini application made in Unity version 2021.3.25f1

The objective is to show a grid formed from an API and then allow the user to
input start coordinates where a drone would be located, a pickup point of the
material and the delivery point.

Since the API has no distance data and only time, we will need to use Dijkstras
pathfinding algorithm instead of A*.


Sources:
* https://www.udacity.com/blog/2021/10/implementing-dijkstras-algorithm-in-python.html
