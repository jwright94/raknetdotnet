#ifndef __ReplicaManagerExt_H_
#define __ReplicaManagerExt_H_

#include "ReplicaManager.h"

class ReplicaManagerWithBackdoor : public ReplicaManager
{
public:
	// Plugin interface functions
	void Update(RakPeerInterface *peer) { ReplicaManager::Update(peer); }
	void OnAttach(RakPeerInterface *peer) { ReplicaManager::OnAttach(peer); }
	PluginReceiveResult OnReceive(RakPeerInterface *peer, Packet *packet) { return ReplicaManager::OnReceive(peer, packet); }
	void OnCloseConnection(RakPeerInterface *peer, SystemAddress systemAddress) { ReplicaManager::OnCloseConnection(peer, systemAddress); }
	void OnShutdown(RakPeerInterface *peer) { ReplicaManager::OnShutdown(peer); }
};

class ReplicaManagerExt;

typedef ReplicaReturnResult (* RME_FP_ConstructionCB)(RakNet::BitStream *inBitStream, RakNetTime timestamp, NetworkID *new_networkID, NetworkIDObject *existingObject, SystemAddress *new_senderId, ReplicaManagerExt *caller, void *userData);
typedef ReplicaReturnResult (* RME_FP_SendDownloadCompleteCB)(RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress *new_senderId, ReplicaManagerExt *caller, void *userData);
typedef ReplicaReturnResult (* RME_FP_ReceiveDownloadCompleteCB)(RakNet::BitStream *inBitStream, SystemAddress* new_senderId, ReplicaManagerExt *caller, void *userData);

class ReplicaManagerExt : public PluginInterface
{
public:
	ReplicaManagerExt() : mediator(this)
	{
		_constructionCB = 0;
		_sendDownloadCompleteCB = 0;
		_receiveDownloadCompleteCB = 0;
		receiveDownloadCompleteUserData = 0;
		sendDownloadCompleteUserData = 0;
		constructionUserData = 0;

		myOBJ.SetReceiveConstructionCB(&mediator);
		myOBJ.SetDownloadCompleteCB(&mediator, &mediator);
	}

	// extend
	void SetReceiveConstructionCB(void *_constructionUserData, RME_FP_ConstructionCB constructionCB) 
	{
		// Just overwrite the construction callback pointer
		_constructionCB=constructionCB;
		constructionUserData=_constructionUserData;	
	}
	void SetDownloadCompleteCB(void *_sendDownloadCompleteUserData, RME_FP_SendDownloadCompleteCB sendDownloadCompleteCB, 
		void *_receiveDownloadCompleteUserData, RME_FP_ReceiveDownloadCompleteCB receiveDownloadCompleteCB) 
	{
		// Just overwrite the send and receive download complete pointers.
		_sendDownloadCompleteCB=sendDownloadCompleteCB;
		_receiveDownloadCompleteCB=receiveDownloadCompleteCB;

		sendDownloadCompleteUserData=_sendDownloadCompleteUserData;
		receiveDownloadCompleteUserData=_receiveDownloadCompleteUserData;
	}

	// Original interfaces
	void SetAutoParticipateNewConnections(bool autoAdd) { myOBJ.SetAutoParticipateNewConnections(autoAdd); }
	bool AddParticipant(SystemAddress systemAddress) { return myOBJ.AddParticipant(systemAddress); }
	bool RemoveParticipant(SystemAddress systemAddress) { return myOBJ.RemoveParticipant(systemAddress); }
	void Construct(Replica *replica, bool isCopy, SystemAddress systemAddress, bool broadcast) { myOBJ.Construct(replica, isCopy, systemAddress, broadcast); }
	void Destruct(Replica *replica, SystemAddress systemAddress, bool broadcast) { myOBJ.Destruct(replica, systemAddress, broadcast); }
	void ReferencePointer(Replica *replica) { myOBJ.ReferencePointer(replica); }
	void DereferencePointer(Replica *replica) { myOBJ.DereferencePointer(replica); }
	void SetScope(Replica *replica, bool inScope, SystemAddress systemAddress, bool broadcast) { myOBJ.SetScope(replica, inScope, systemAddress, broadcast); }
	void SignalSerializeNeeded(Replica *replica, SystemAddress systemAddress, bool broadcast) { myOBJ.SignalSerializeNeeded(replica, systemAddress, broadcast); }
	// hooks these methods
	//void SetReceiveConstructionCB(ReceiveConstructionInterface *receiveConstructionInterface);
	//void SetDownloadCompleteCB( SendDownloadCompleteInterface *sendDownloadComplete, ReceiveDownloadCompleteInterface *receiveDownloadComplete );
	void SetSendChannel(unsigned char channel) { myOBJ.SetSendChannel(channel); }
	void SetAutoConstructToNewParticipants(bool autoConstruct) { myOBJ.SetAutoConstructToNewParticipants(autoConstruct); }
	void SetDefaultScope(bool scope) { myOBJ.SetDefaultScope(scope); }
	void SetAutoSerializeInScope(bool autoSerialize) { myOBJ.SetAutoSerializeInScope(autoSerialize); }
	void EnableReplicaInterfaces(Replica *replica, unsigned char interfaceFlags) { myOBJ.EnableReplicaInterfaces(replica, interfaceFlags); }
	void DisableReplicaInterfaces(Replica *replica, unsigned char interfaceFlags) { myOBJ.DisableReplicaInterfaces(replica, interfaceFlags); } 
	bool IsConstructed(Replica *replica, SystemAddress systemAddress) { return myOBJ.IsConstructed(replica, systemAddress); }
	bool IsInScope(Replica *replica, SystemAddress systemAddress) { return myOBJ.IsInScope(replica, systemAddress); }
	unsigned GetReplicaCount(void) const { return myOBJ.GetReplicaCount(); }
	Replica *GetReplicaAtIndex(unsigned index) { return myOBJ.GetReplicaAtIndex(index); }
	unsigned GetParticipantCount(void) const { return myOBJ.GetParticipantCount(); }
	SystemAddress GetParticipantAtIndex(unsigned index) { return myOBJ.GetParticipantAtIndex(index); }
	bool HasParticipant(SystemAddress systemAddress) { return myOBJ.HasParticipant(systemAddress); }
	void SignalSerializationFlags(Replica *replica, SystemAddress systemAddress, bool broadcast, bool set, unsigned int flags) { myOBJ.SignalSerializationFlags(replica, systemAddress, broadcast, set, flags); }
	unsigned int* AccessSerializationFlags(Replica *replica, SystemAddress systemAddress) { return myOBJ.AccessSerializationFlags(replica, systemAddress); }

protected:
	// Plugin interface functions
	void Update(RakPeerInterface *peer) { myOBJ.Update(peer); }
	void OnAttach(RakPeerInterface *peer) { myOBJ.OnAttach(peer); }
	PluginReceiveResult OnReceive(RakPeerInterface *peer, Packet *packet) { return myOBJ.OnReceive(peer, packet); }
	void OnCloseConnection(RakPeerInterface *peer, SystemAddress systemAddress) { myOBJ.OnCloseConnection(peer, systemAddress); }
	void OnShutdown(RakPeerInterface *peer) { myOBJ.OnShutdown(peer); }

	class Mediator : public  ReceiveConstructionInterface, public SendDownloadCompleteInterface, public ReceiveDownloadCompleteInterface
	{
	public:
		Mediator(ReplicaManagerExt* _ext)
		{
			ext = _ext;
		}
		ReplicaReturnResult ReceiveConstruction(RakNet::BitStream *inBitStream, RakNetTime timestamp, NetworkID networkID, NetworkIDObject *existingObject, SystemAddress senderId, ReplicaManager *caller)
		{
			return ext->_constructionCB(inBitStream, timestamp, new NetworkID(networkID), existingObject, new SystemAddress(senderId), ext, ext->constructionUserData);
		}
		ReplicaReturnResult SendDownloadComplete(RakNet::BitStream *outBitStream, RakNetTime currentTime, SystemAddress senderId, ReplicaManager *caller)
		{
			return ext->_sendDownloadCompleteCB(outBitStream, currentTime, new SystemAddress(senderId), ext, ext->sendDownloadCompleteUserData);
		}
		ReplicaReturnResult ReceiveDownloadComplete(RakNet::BitStream *inBitStream, SystemAddress senderId, ReplicaManager *caller)
		{
			return ext->_receiveDownloadCompleteCB(inBitStream, new SystemAddress(senderId), ext, ext->receiveDownloadCompleteUserData);
		}
	protected:
		ReplicaManagerExt* ext;
	};

	// Required callback to handle construction calls
	RME_FP_ConstructionCB _constructionCB;

	// Optional callbacks to send and receive download complete.
	RME_FP_SendDownloadCompleteCB _sendDownloadCompleteCB;
	RME_FP_ReceiveDownloadCompleteCB _receiveDownloadCompleteCB;

	// Userdata with the callbacks
	void *receiveDownloadCompleteUserData, *sendDownloadCompleteUserData, *constructionUserData;

	Mediator mediator;
	ReplicaManagerWithBackdoor myOBJ;
};

#endif //__ReplicaManagerExt_H_