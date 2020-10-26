const px = [0.15, 0.07, 0.09, 0.08, 0.09, 0.12, 0.04, 0.13, 0.12, 0.10];

const findEntropy = (arr) => {
    let res = 0;
    for (let x of arr) {
        const log = Math.log(x) / Math.log(2);
        res += x * log
    }
    return -res;
}

console.log(findEntropy(px));