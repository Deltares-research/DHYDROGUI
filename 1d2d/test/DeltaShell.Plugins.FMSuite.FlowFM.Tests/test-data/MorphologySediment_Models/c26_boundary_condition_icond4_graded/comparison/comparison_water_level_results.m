try 
    oetroot;
catch 
    oetsettings;
end

for timestep = [2];
    
    d3d_paths    = {'../delft3d/'};
    dfm_paths    = {'../dflowfmoutput/'};
    d3d_versions = {'6.01.17.5207'};  % '6.02.02.5598M',
    dfm_versions = {'1.1.156.44836'};
    
    d3d_quantity_names = {'water level', ...
                          'bed level in water level points'};
    dfm_quantity_names = {'water level - nmesh2d_face: mean', ...
                          'Time-varying bottom level in flow cell centers - nmesh2d_face: mean'};

    %ylims = {[.15 0.55],[-1.0 -0.3]};
    colors = linspecer(length(d3d_versions)+length(dfm_versions),'qualitative');
        
    
    for quantityno = 1:2;
        
        %Delft3D reference result
        j = 0;
        for k = 1:length(d3d_paths);
            j = j+1;
            d3d_qp('openfile',[d3d_paths{k},'trim-str.dat'])
            d3d_qp('selectfield',d3d_quantity_names{quantityno})
            d3d_qp('editxy','-3.200, -5.000; -3.200, 11.000')
            d3d_qp('editt',timestep)
            d3d_qp('colour',colors(j,:))
            if j == 1;
                d3d_qp('quickview')
            else
                d3d_qp('addtoplot')
            end
            legendstr{j} = ['Delft3D-4 ' d3d_versions{k}];
        end
        
        %DFlow-FM
        for k = 1:length(dfm_paths);
            j = j+1;
            legendstr{j} = ['Delft3D-FM ' dfm_versions{k}];
            d3d_qp('openfile',[dfm_paths{k},'str_map.nc'])
            d3d_qp('selectfield',dfm_quantity_names{quantityno})
            d3d_qp('editxy','-3.200, -5.000; -3.200, 11.000')
            d3d_qp('editt',timestep)
            d3d_qp('colour',colors(j,:))
            d3d_qp('addtoplot')
        end
        title([d3d_quantity_names{quantityno},', Timestep = ',num2str(timestep)])
        legend(legendstr)
        %ylim(ylims{quantityno});
        box on;
        
        d3d_qp('printfigure',['comparison_d3d_fm_',num2str(quantityno),'.png'],'PNG file',2,300,1,1,1)    
    end
end