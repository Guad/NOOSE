def e(path):
    # IN: new Tuple<Vector3, float>(new Vector3(-1814.42f, -1212.90f, 13.017f), 0f),
    # OUT: <Enemy x="62.58003" y="3700.969" z="39.7524" heading="289.8477"/>
    muh = open(path, "r")
    lines = muh.read()
    output = ""
    for line in lines.splitlines():
        ayy = line
        ayy = ayy.rstrip("\n")
        ayy = ayy.replace("new Tuple<Vector3, float>(new Vector3(", "").replace(")", "")
        outp = f(ayy)
        output += "<Enemy " + outp + "/>\n"
    muh.close()
    return output

def f(s):
    output = ""
    l = s.split(" ")
    c = 0
    arr = ['x=', 'y=', 'z=', 'heading=']
    for word in l:
        word = word.replace("f", "").replace(",", "")
        output += arr[c] + "\"" + word + "\" "
        c += 1
    return output
    
print(e("input.txt"))
    
while True:
    inp = input(">>")
    print(f(inp))