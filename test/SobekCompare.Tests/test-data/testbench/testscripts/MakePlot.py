
# coding: utf-8

# In[1]:

import os.path
import sys
import logging

import pandas
import matplotlib.pyplot as plt
#import matplotlib.style


# In[35]:

# use this stylesheet
#matplotlib.style.use('ggplot')

# get the filename from the first command line argument (0=script, 1=filename)
filename = sys.argv[1]
try:
    assert os.path.isfile(filename), 'First command line argument should be a filename, %s is not a file' % (filename, )
except AssertionError:
    logging.exception("Command line error")
    filename = '/Users/baart_f/data/sobek/Totalarea_h_x1800m.fix'
    #filename = '/Users/baart_f/data/sobek/Discharge_B2.fix'


# In[36]:

# read file, assuming multiple space separted columns (no spaces in parameters or locations)
widths = [25, 25, 11, 14, 18, 18, 14]
df = pandas.read_fwf(filename, widths=widths,  skiprows=2, parse_dates=[['Date', 'Time']])

# generate 1 row, 2 columns
fig, axes = plt.subplots(1, 2, figsize=(13, 5.5))

# assuming only 1 parameter, location per file
parameters = set(df['Parameter'])
locations = set(df['Location'])
assert len(locations) == 1, 'Expected 1 location'
assert len(parameters) == 1, 'Expected exactly 1 parameter'
location, parameter = list(locations)[0], list(parameters)[0]

# plot in the two axes
df.plot('Date_Time', ['Diff'], ax=axes[0])
axes[0].set_title(location)
axes[0].set_ylabel(parameter)
axes[0].set_xlabel('time')
df.plot('Date_Time', ['SOBEK2', 'SOBEK3'], ax=axes[1])
axes[1].set_title(location)
axes[1].set_ylabel(parameter)
axes[1].set_xlabel('time')
fig.tight_layout()

# save the figure
outfilename = '{parameter} {location}.png'.format(location=location, parameter=parameter)
fig.savefig(outfilename)
