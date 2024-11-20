function [status, VARID] = definevar(NCID, VarName, VarType, Dims, Attribs)
[VARID, status] = mexnc('def_var',NCID,VarName,VarType,length(Dims),Dims);
if status~=0
    return
end
if nargin>4
    for i = 1:size(Attribs,1)
        AName = Attribs{i,1};
        AType = Attribs{i,2};
        AVal  = Attribs{i,3};
        %
        switch AType
            case nc_char
                cmd = 'put_att_text';
            otherwise
                cmd = 'put_att_double';
        end
        status = mexnc(cmd,NCID,VARID,AName,AType,length(AVal),AVal);
        if status~=0
            return
        end
    end
end
