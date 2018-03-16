REM SET nvcc=C:\CUDA8\bin\nvcc.exe
REM SET nvcc9=C:\CUDA9\bin\nvcc.exe
REM SET typechoice=%1
REM 
REM IF "%typechoice%"=="standalone" (
REM 	SET cyclesroot=D:\Dev\CCSycles
REM 	SET cyclesout=D:\Dev\CCSycles
REM ) else (
REM 	SET cyclesroot=D:\Dev\Rhino\rhino\src4\rhino4\Plug-ins\RDK\cycles
REM 	SET cyclesout=D:\Dev\Rhino\rhino\big_libs\RhinoCycles
REM )
REM 
REM SHIFT
REM 
REM IF NOT EXIST "%cyclesroot%/lib" (
REM 	MKDIR "%cyclesroot%/lib"
REM )
REM 
REM SET cudaversion=8
REM SET cudaversion75=8
REM SET shadermodelnum=%1
REM SET shadermodel=sm_%shadermodelnum%

IF [%shadermodelnum%]==[] (
	FOR %%s IN ("sm_20", "sm_21") DO (
		"%nvcc%" -arch=%%s -m64 --cubin %cyclesroot%/cycles/src/kernel/kernels/cuda/kernel.cu -o %cyclesout%/lib/kernel_%%s.cubin --ptxas-options="-v" -D__KERNEL_CUDA_VERSION__=%cudaversion% --use_fast_math -I%cyclesroot%/cycles/src -DCCL_NAMESPACE_BEGIN= -DCCL_NAMESPACE_END= -DNVCC -D__NO_CAMERA_MOTION__ -D__NO_OBJECT_MOTION__ -D__NO_HAIR__ -D__NO_BAKING__ -D__NO_VOLUME__ -D__NO_BRANCHED_PATH__ -D__NO_PATCH_EVAL__ -D__NO_DENOISING__
	)
	FOR %%s IN ("sm_30", "sm_35", "sm_50", "sm_52", "sm_60", "sm_61") DO (
		"%nvcc%" -arch=%%s -m64 --cubin %cyclesroot%/cycles/src/kernel/kernels/cuda/kernel.cu -o %cyclesout%/lib/kernel_%%s.cubin --ptxas-options="-v" -D__KERNEL_CUDA_VERSION__=%cudaversion% --use_fast_math -I%cyclesroot%/cycles/src -DCCL_NAMESPACE_BEGIN= -DCCL_NAMESPACE_END= -DNVCC -D__NO_CAMERA_MOTION__ -D__NO_OBJECT_MOTION__ -D__NO_HAIR__ -D__NO_BAKING__ -D__NO_VOLUME__ -D__NO_BRANCHED_PATH__ -D__NO_PATCH_EVAL__ -D__NO_DENOISING__
	)
) ELSE (
	"%nvcc%" -arch=%shadermodel% -m64 --cubin %cyclesroot%/cycles/src/kernel/kernels/cuda/kernel.cu -o %cyclesout%/lib/kernel_%shadermodel%.cubin --ptxas-options="-v" -D__KERNEL_CUDA_VERSION__=%cudaversion% --use_fast_math -I%cyclesroot%/cycles/src -DCCL_NAMESPACE_BEGIN= -DCCL_NAMESPACE_END= -D__NO_CAMERA_MOTION__ -D__NO_OBJECT_MOTION__ -DNVCC -D__NO_HAIR__ -D__NO_BAKING__ -D__NO_VOLUME__ -D__NO_BRANCHED_PATH__ -D__NO_PATCH_EVAL__ -D__NO_DENOISING__
)
