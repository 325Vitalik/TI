import math

InputArr = [0.15, 0.07, 0.09, 0.08, 0.09, 0.12, 0.04, 0.13, 0.12, 0.10]

def entropy(probs):
    res = 0
    for i in probs:
        res += i * math.log2(i)

    return -res

print(entropy(InputArr))