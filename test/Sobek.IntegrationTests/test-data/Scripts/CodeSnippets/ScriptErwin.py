## importeer Python tijd en systeem bibliotheken
import time
import System
## importeer Delta Shell bibliotheken
from System.Collections.Generic import List
from DelftTools.Functions import *

## Haal het integrated model op
im = CurrentProject.RootFolder["integrated model"]
## Afhankelijk van de volgorde in je project tree kun je je flow en rtc modellen ophalen
flow = im.Activities[0]
rtc = im.Activities[1]

## Creeer een array van lengte het aantal runs wat je wilt doen met bijbehorende stoptijden
dates = [System.DateTime(2007,1,15,1,0,0),System.DateTime(2007,1,15,9,0,0),System.DateTime(2007,1,15,17,0,0),System.DateTime(2007,1,16,1,0,0),System.DateTime(2007,1,16,9,0,0),System.DateTime(2007,1,16,17,0,0),System.DateTime(2007,1,17,1,0,0)]

## Maak een tijdserie om te plotten (misbruik FlowTimeSeries)
ts = List[IFunction]()
ts = CreateFlowTimeSeries()
   
## Maak een loop over de lengte van je array
for i in range(len(dates)):
   ## Starttijd
   start = time.time()
   ## Pas eindtijd van flow model aan
   flow.StopTime = dates[i]
   ## Draai het integrated model
   Application.RunActivity(im)
   ## Stoptijd
   stop = time.time()
   ## Totale run tijd
   total = stop - start
   ## Voeg waarden voor deze run toe aan tijdserie
   ts[dates[i]] = total

## Plot resultaten (werkt enkel als script binnen de GUI wordt gedraaid)
Gui.CommandHandler.OpenView(timeseries)

## Sla je project eventueel op
Application.SaveProjectAs("c:\Users\putten_hs\Desktop\REWaal_test.dsproj")
   