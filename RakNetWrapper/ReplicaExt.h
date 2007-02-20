#ifndef __ReplicaExt_H_
#define __ReplicaExt_H_

#include "Replica.h"

// see http://www.mono-project.com/Interop_with_Native_Libraries#Boolean_Members
class ReplicaBoolMarshalAsUInt : public Replica
{
public:
	// These methods accept unsigned int.
	virtual ReplicaReturnResult SendConstruction( RakNetTime currentTime, SystemAddress systemAddress, unsigned int &flags, RakNet::BitStream *outBitStream, unsigned int *includeTimestamp )=0;
	virtual ReplicaReturnResult SendDestruction(RakNet::BitStream *outBitStream, SystemAddress systemAddress, unsigned int *includeTimestamp )=0;
	virtual ReplicaReturnResult SendScopeChange(bool inScope, RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress systemAddress, unsigned int *includeTimestamp )=0;
	virtual ReplicaReturnResult Serialize(unsigned int *sendTimestamp, RakNet::BitStream *outBitStream, RakNetTime lastSendTime, PacketPriority *priority, PacketReliability *reliability, RakNetTime currentTime, SystemAddress systemAddress, unsigned int &flags)=0;

protected:
	// I manually convert the boolean to unsigned int.
	ReplicaReturnResult SendConstruction( RakNetTime currentTime, SystemAddress systemAddress, unsigned int &flags, RakNet::BitStream *outBitStream, bool *includeTimestamp )
	{
		IMBoolean imIncludeTimestamp(includeTimestamp);
		return SendConstruction(currentTime, systemAddress, flags, outBitStream, &imIncludeTimestamp.integer);
	}
	ReplicaReturnResult SendDestruction(RakNet::BitStream *outBitStream, SystemAddress systemAddress, bool *includeTimestamp )
	{
		IMBoolean imIncludeTimestamp(includeTimestamp);
		return SendDestruction(outBitStream, systemAddress, &imIncludeTimestamp.integer);
	}
	ReplicaReturnResult SendScopeChange(bool inScope, RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress systemAddress, bool *includeTimestamp )
	{
		IMBoolean imIncludeTimestamp(includeTimestamp);
		return SendScopeChange(inScope, outBitStream, currentTime, systemAddress, &imIncludeTimestamp.integer);
	}
	ReplicaReturnResult Serialize(bool *sendTimestamp, RakNet::BitStream *outBitStream, RakNetTime lastSendTime, PacketPriority *priority, PacketReliability *reliability, RakNetTime currentTime, SystemAddress systemAddress, unsigned int &flags)
	{
		IMBoolean imSendTimestamp(sendTimestamp);
		return Serialize(&imSendTimestamp.integer, outBitStream, lastSendTime, priority, reliability, currentTime, systemAddress, flags);
	}

	struct IMBoolean
	{
		// bool to unsigned int
		IMBoolean(bool* _from)
		{
			from = _from;
			integer = *from ? 1 : 0;
		}
		// unsigned int to bool
		~IMBoolean()
		{
			*from = integer ? true : false;
		}

		bool* from;
		unsigned int integer;  // intermediate expression
	};
};

#endif //__ReplicaExt_H_