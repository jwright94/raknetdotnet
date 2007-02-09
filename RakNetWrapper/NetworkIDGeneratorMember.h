#ifndef __NetworkIDGeneratorMember_H_
#define __NetworkIDGeneratorMember_H_

class NetworkIDGeneratorMember : public NetworkIDGenerator
{
public:	
	NetworkIDGeneratorMember() { isAuthority = false; }
	NetworkIDGeneratorMember(bool _isAuthority) { isAuthority = _isAuthority; }
	virtual bool IsNetworkIDAuthority(void) const {return isAuthority;}
	virtual void SetNetworkIDAuthority(bool _isAuthority) { isAuthority = _isAuthority; }
protected:
	bool isAuthority;
};

#endif //__NetworkIDGeneratorMember_H_