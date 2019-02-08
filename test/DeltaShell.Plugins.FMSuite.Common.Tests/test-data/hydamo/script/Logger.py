# coding: latin-1
import sys,time

class Logger(object):
    def __init__(self, filename="Default.log"):
        self.terminal = sys.stdout
        f = open(filename, 'w')

        f.write('*********    ' + time.strftime("%Y-%m-%d %H:%M") + '     *********\n')
        f.close()
        self.log = open(filename, 'a')

    def write(self, message):
        self.terminal.write(message)
        self.log.write(message)

    def flush(self):
        self.terminal.write('Flush')
        self.log.write('Flush')