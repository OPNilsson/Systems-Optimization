This document gives the description of tags used in the input files.


%%%%%%%%%%%%%%%%%%%%%%%%%% Apps.xml %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
Tags	:	Description

Application	:	Parnet object of the applications
Message		:	Parent object for flow
Name		:	Identifier of the flow
Source		:	Talker node of the flow
Destination	:	Listener node of the flow
Size		: 	Size of the flow (in Bytes)
Period		:	Period of the flow (in us)
Deadline	:	Deadline of the flow (in us)



%%%%%%%%%%%%%%%%%%%%%%%% Config.xml %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
Tags	:	Description

Architecture	:	Parnet object of the architecture
Vertex			:	Parent object for node
Name			:	Identifier of the node
Edge			:	Parent object for link
Id				:	Identifier of the link
BW				: 	Maximum bandwidth of the link (in Mbps)
PropDelay		:	Propagation delay of the link (in us)
Source			:	Origin node of the link
End				:	End node of the link