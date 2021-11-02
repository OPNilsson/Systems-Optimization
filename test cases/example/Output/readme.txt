This document gives the description of tags used in the output file.

Tags	:	Description

Solution	:	Parent object of the solution
Runtime		:	Runtime of the solution (seconds)
MeanE2E		: 	Mean normalized end-to-end delay of all flows (multiply by 1000)
MeanBW		:	Mean normalized maximum bandwidth utilization of all links (multiply by 1000)

Message		: 	Parent object for flow
Name		:	Identifier of the flow
MaxE2E		: 	Maximum end-to-end delay of the flow (in us)
Link		:	Link assignment object of the route
Source		:	Source node of the link
Destination	:	Destination node of the link
Qnumber		:	Associated queue number of the link (starts from 1)