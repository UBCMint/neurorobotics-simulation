import random
import time

def produce_random():
    while True:
        data = random.randint(0, 1)
        print(data)
        with open("../shared_data.txt", "w") as f:
            f.write(str(data))
        time.sleep(1)  # wait 1 second

if __name__ == "__main__":
    produce_random()
