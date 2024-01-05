# MyMinecraft
MyMinecraft

Scritpable-InventorySystem 에서 만든 여러 시스템을 가지고와 

마인크래프트와 합쳐서 만드는 프로젝트

///////////////////////////////////////////////////////////////////////////////////////////

inventory System에 container 부분을 scriptable object를 빼고 그냥 클래스로 바꿈

아이템의 종류가 블록을 설치하는 아이템이 있고 장착하는 아이템이 있는데 이걸 item 클래스에 다 박아놓고

만들려고 하니 너무 불편함

결국 장비아이템 설치아이템 소모아이템을 따로 클래스를 만들어서 만들기로 하고

container가 가장 부모 클래스인 item을 저장해봤자 또 item의 부분만 저장되고 자식 클래스의 정보까진 저장을 못하니

interface를 만들어서 저장하는 방법을 사용해봤고 잘 작동해서 채택함

container는 scriptable object가 아닌 일반 클래스고 저장하는건 item이 아닌 itemInterface임

itemInterface에 상속받은 아이템 속성을 리턴하는 함수를 만들려고 했는데

getComponumt가 불가능해서 곤란했지만 그냥 return (T)this를 했고 오류가 남

T 가 this가 불가능한 클래스일지도 모른다는게 gpt의 의견 그래서 try catch로 가능하면 this 아니면 null을 리턴하도록 함

///////////////////////////////////////////////////////////////////////////////////////////

ArgumentNullException: Value cannot be null. 

Parameter name: _unity_self

This has been fixed by an unknown PR.
Confirmed that 2023.1.0b9 has the issue however later versions do not.
Tested 2023.1.5f1, 2023.2.0b9 and 2023.3.0a1.

Note: This bug is only looking into the error with regards to the preset window, not the error message in general which can occur under many different circumstances.

스크립트블 오브젝트때문에 생기는 에러같음

