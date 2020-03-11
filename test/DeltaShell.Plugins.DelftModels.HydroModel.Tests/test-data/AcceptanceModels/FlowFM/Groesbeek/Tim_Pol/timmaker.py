import glob, os
import pandas as pd
import tkinter as tk
from tkinter import filedialog

root = tk.Tk()
root.withdraw()

file_path = filedialog.askopenfilename(filetypes =(("xlsx File", "*.xlsx"),("All Files","*.*")),
                           title = "Choose a file.")                                           # opent .xls waar debiet data per put staat

timdata = pd.read_excel(file_path)                                                             # leest scel bestand in
timdata_cols = timdata.columns.tolist()


a = file_path.rsplit('/', 1)[0]   
files_found = []
os.chdir(a)
for file in glob.glob("*.pol"):                                                                # vindt alle .pol bestanden in werkmap
    files_found += [file[:-4]]

tijd = timdata[timdata.columns[0]]

for location in timdata_cols[1:]:                                                              # loop met alle putten
    for file in files_found:                                                                   # loop met alle gevonden .pol bestanden
        if str(location) in file:                                                              # zoekt op of put een .pol bestand heeft
            
            write_tim = open("{}_0001.tim".format(file), 'w')                                  # maakt voor gevonden put een .tim bestand aan
            
            timline = (timdata[location])
            for i in range(len(timline)): 
                write_tim.write('{} {}\n'.format(i, timline[i]))                               # minuten, debiet
            write_tim.write('999999 0.0  ')                                                    
            write_tim.close()
            print('Wrote {}'.format(file))
            break

print('###############Finished###############')                     

