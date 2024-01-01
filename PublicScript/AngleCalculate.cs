

using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public static class AngleCalculate
{
    //각도로 위치계산
    public static Vector3 SphericalCoordinate(float angle, float distance, bool collision)
    {
        //각도를 수로 변환 후 cos값을 얻기
        float cosY = Mathf.Cos(angle * Mathf.Deg2Rad);
        //... sin값을 얻기
        float sinY = Mathf.Sin(angle * Mathf.Deg2Rad);

        //...
        float cosX = Mathf.Cos(angle * Mathf.Deg2Rad);
        //...
        float sinX = Mathf.Sin(angle * Mathf.Deg2Rad);

        //얻은 cos sin들로 위치를 계산 후 거리를 곱해 위치를 정함
        return new Vector3(sinX, sinY, cosX * cosY) * distance;
    }

    //위치로 각도 계산
    public static float Angle_Calculation_X(Transform criteria, Transform target)
    {
        //x축 기준 (옆에서 봤을때 x 에서 -x쪽을 바라보면서 오른쪽으로 가면 +)
        //자기 오른쪽에 있으면 0도 위에 있으면 90도 아래에 있으면 -90도 (옆에서 봤을때 오른쪽에 있다는것은 앞에 있다는 것) 
        float x = target.position.z - criteria.position.z;
        float y = target.position.y - criteria.position.y;
        float angleInR = Mathf.Atan2(y, x);
        return angleInR * (180 / Mathf.PI);
    }
    public static float Angle_Calculation_Y(Transform criteria, Transform target)
    {
        //Y축 기준 (y 에서 -y를 바라보면서 오른쪽으로 가면 +)
        float x = target.position.x - criteria.position.x;
        float y = target.position.z - criteria.transform.position.z;
        float angleInR = Mathf.Atan2(y, x);
        return angleInR * (180 / Mathf.PI);
    }
    public static float Angle_Calculation_Z(Transform criteria, Transform target)
    {
        //z축 기준 (-z 에서 z를 바라보면서 오른쪽으로 가면 +)
        float x = target.position.x - criteria.transform.position.x;
        float y = target.position.y - criteria.transform.position.y;
        float angleInR = Mathf.Atan2(y, x);
        return angleInR * (180 / Mathf.PI);
        //왜 z축만 -z에서 z를 봐야 하는건지 이유를 모르겠네
    }

    //270도와 -90도는 같은거지만 비교하기 힘들기에
    //각도를 항상 -180 ~ 180도 이내로 만들어 서로 비교하기에 편하게 만듬
    public static float NormalizeAngle(float angle)
    {
        angle %= 360; // 각도를 360도 범위 내로 줄임

        if (angle < -180)
        {
            angle += 360; // -180도 미만일 경우, 180도를 더해 180도 범위 내로 이동
        }
        else if (angle > 180)
        {
            angle -= 360; // 180도 초과일 경우, 180도를 빼 180도 범위 내로 이동
        }

        return angle;
    }

    //입사벡터와 노말벡터를 넣으면 반사벡터를 리턴해줌
    //주의할점은 vector2라는것 vecotr3를 그냥 넣으면 xy값만 들아어고 z값은 안들어와서 이상한 값이 나갈 수 있음
    public static Vector2 ReflexAngle(Vector2 dir, Vector2 normal)
    {
        return Vector2.Reflect(dir, normal);
    }
}