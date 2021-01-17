using UnityEngine;

public class TestSector : MonoBehaviour
{
    public Transform Target;
    public float SkillDistance = 500f;//扇形距离
    public float SkillJiaodu = 60f;//扇形的角度

    void LateUpdate()
    {
        float distance = Vector3.Distance(transform.position, Target.position);//距离
        Vector3 norVec = transform.rotation * Vector3.up;
        Vector3 temVec = Target.position - transform.position;
        Debug.DrawLine(transform.position, norVec, Color.red);//画出技能释放者面对的方向向量
        Debug.DrawLine(transform.position, Target.position, Color.green);//画出技能释放者与目标点的连线
        //float jiajiao = Mathf.Acos(Vector3.Dot(norVec.normalized, temVec.normalized)) * Mathf.Rad2Deg;//计算两个向量间的夹角
        float jiajiao = Vector2.Angle(norVec.normalized, temVec.normalized);//计算两个向量间的夹角
        if((distance < SkillDistance) && (jiajiao <= SkillJiaodu * 0.5f))
        {
            Debug.Log("在扇形范围内");
        }
    }
}
