# How to - agent training

Launch anaconda prompt in the desired env, and enter project:

```
conda activate aijam
D:
cd D:\PMI2 - AI JAM\AI-JAM-2025-Unity-Project\AI-JAM-2025
```

## How to run the agent

```
mlagents-learn --run-id={run_id}
mlagents-learn --run-id={run_id} --resume
mlagents-learn --run-id={run_id} --force
mlagents-learn configs/SelfPlay.yaml --run-id=FasterRobots --force
mlagents-learn configs/SelfPlay.yaml --run-id=FasterRobots --resume
```
