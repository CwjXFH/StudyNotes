function* promiseAll(values) {
    for (let i = 0; i < values.length; i++) {
        // yield values[i];
        let result = new Promise(resolve => {
            setTimeout(() => resolve(values[i]), 3000);
        }).then(value => {
            console.log(`func1: ${value}`);
            return value;
        });
        yield result;
        console.log(result);
    }
}

function func1() {
    return new Promise(resolve => {
        setTimeout(() => resolve(100), 3000);
    }).then(value => {
        console.log(`func1: ${value}`);
        return value;
    });
}

function func2() {
    return new Promise(resolve => {
        setTimeout(() => resolve(200), 3000);
    }).then(value => {
        console.log(`func2: ${value}`);
        return value;
    });
}

// let all = promiseAll([func1(), func2()]);
// let p1 = all.next();
// func1();
// func2();
// setTimeout(() => console.log('over'), 2000);
// new Promise(resolve => {
//     setTimeout(() => resolve(100), 2000);
// })
//     .then(value => console.log(value));

let all = promiseAll([100, 200]);
let p1 = all.next();
// all.next();
console.log();
