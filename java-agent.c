#include "jvmti.h"
#include "stdio.h"
#include "string.h"

//a simple java agent

static void JNICALL callbackVMObjectAlloc
    (jvmtiEnv *jvmti_env, JNIEnv* jni_env, jthread thread, 
             jobject object, jclass object_klass, jlong size) 
{
                 printf("memory allocated...\n");
                (*jvmti_env)->SetEventNotificationMode(jvmti_env, JVMTI_DISABLE, JVMTI_EVENT_VM_OBJECT_ALLOC, NULL);
}

static void JNICALL
ObjectFree(jvmtiEnv *jvmti_env,
            jlong tag)
            {
                
            }

static void JNICALL
ClassLoad(jvmtiEnv *jvmti_env,
            JNIEnv* jni_env,
            jthread thread,
            jclass klass)
{
    printf("class loaded...\n");
}


int objectsFound = 0;
int objectsFromClass = 0;

jvmtiIterationControl JNICALL Objectfound(jlong class_tag, 
     jlong size, 
     jlong* tag_ptr, 
     void* user_data)
     {
         (*tag_ptr) = 2345;
         printf("Ob:%s\n", (char*)user_data);
         objectsFromClass++;
     }

JNICALL jvmtiIterationControl HeapRoot(jvmtiHeapRootKind root_kind, 
     jlong class_tag, 
     jlong size, 
     jlong* tag_ptr, 

     void* user_data)
     {
        (*tag_ptr) = 1234;
        objectsFound++;
     }

jvmtiIterationControl JNICALL HeapObject(jlong class_tag, 
     jlong size, 
     jlong* tag_ptr, 
     void* user_data)
     {
        (*tag_ptr) = 1234;
        objectsFound++;
     }

jvmtiIterationControl JNICALL HeapReferences(jvmtiObjectReferenceKind reference_kind, 
     jlong class_tag, 
     jlong size, 
     jlong* tag_ptr, 
     jlong referrer_tag, 
     jint referrer_index, 
     void* user_data)
{
    (*tag_ptr) = 1234;
    objectsFound++;
}

jvmtiIterationControl JNICALL StackReferences(jvmtiHeapRootKind root_kind, 
     jlong class_tag, 
     jlong size, 
     jlong* tag_ptr, 
     jlong thread_tag, 
     jint depth, 
     jmethodID method, 
     jint slot, 
     void* user_data)
     {
        (*tag_ptr) = 1234;
        objectsFound++;
     }


JNIEXPORT jint /*JNICALL*/ Agent_OnAttach(JavaVM *jvm, char *options, void *reserved) {

    jvmtiEnv* jvmti = NULL;
    JNIEnv* jni = NULL;

    (*jvm)->GetEnv(jvm, (void**)&jvmti, JVMTI_VERSION_1_2);
    (*jvm)->GetEnv(jvm, (void**)&jni, JNI_VERSION_1_1);

 
    jvmtiCapabilities capa = {0};
    (*jvmti)->GetPotentialCapabilities(jvmti, &capa);

    printf("hello world");

/*
    FILE *out_file = fopen("D:\\gay.txt", "w"); // write only 
    fprintf(out_file, "object tagging %d\n", capa.can_tag_objects);
    fprintf(out_file, "vm alloc %d\n", capa.can_generate_vm_object_alloc_events);

    fclose(out_file);

    printf("value 1 %d \n", capa.can_tag_objects );
    printf("value %d \n", capa.can_generate_field_modification_events );
    printf("value %d \n", capa.can_generate_field_access_events );
    printf("value %d \n", capa.can_get_bytecodes );
    printf("value %d \n", capa.can_get_synthetic_attribute );
    printf("value %d \n", capa.can_get_owned_monitor_info );
    printf("value %d \n", capa.can_get_current_contended_monitor);
    printf("value %d \n", capa.can_get_monitor_info );
    printf("value %d \n", capa.can_pop_frame );
    printf("value 10 %d \n", capa.can_redefine_classes );

    printf("value 11 %d \n", capa.can_signal_thread );
    printf("value %d \n", capa.can_get_source_file_name );
    printf("value %d \n", capa.can_get_line_numbers );
    printf("value %d \n", capa.can_get_source_debug_extension );
    printf("value %d \n", capa.can_access_local_variables );
    printf("value %d \n", capa.can_maintain_original_method_order );
    printf("value %d \n", capa.can_generate_single_step_events );
    printf("value %d \n", capa.can_generate_exception_events );
    printf("value %d \n", capa.can_generate_frame_pop_events );
    printf("value 20 %d \n", capa.can_generate_breakpoint_events );

    printf("value %d \n", capa.can_suspend );
    printf("value %d \n", capa.can_redefine_any_class );
    printf("value %d \n", capa.can_get_current_thread_cpu_time );
    printf("value %d \n", capa.can_get_thread_cpu_time );
    printf("value %d \n", capa.can_generate_method_entry_events );
    printf("value %d \n", capa.can_generate_method_exit_events );
    printf("value %d \n", capa.can_generate_all_class_hook_events );
    printf("value %d \n", capa.can_generate_compiled_method_load_events );
    printf("value %d \n", capa.can_generate_monitor_events );
    printf("value 30 %d \n", capa.can_generate_vm_object_alloc_events );
    
    printf("value %d \n", capa.can_generate_native_method_bind_events  );
    printf("value %d \n", capa.can_generate_garbage_collection_events  );
    printf("value %d \n", capa.can_generate_object_free_events  );
    printf("value %d \n", capa.can_force_early_return  );
    printf("value %d \n", capa.can_get_owned_monitor_stack_depth_info  );
    printf("value %d \n", capa.can_get_constant_pool  );
    printf("value %d \n", capa.can_set_native_method_prefix  );
    printf("value %d \n", capa.can_retransform_classes  );
    printf("value %d \n", capa.can_retransform_any_class  );
    printf("value 40 %d \n", capa.can_generate_resource_exhaustion_heap_events  );
    
    printf("value 41 %d \n", capa.can_generate_resource_exhaustion_threads_events  );
*/

    int err = (*jvmti)->AddCapabilities(jvmti, &capa);
    printf("error %d", err);

//events----------------

    (*jvmti)->SetEventNotificationMode
      (jvmti, JVMTI_ENABLE, JVMTI_EVENT_VM_OBJECT_ALLOC, (jthread)NULL);

    (*jvmti)->SetEventNotificationMode
      (jvmti, JVMTI_ENABLE, JVMTI_EVENT_CLASS_LOAD, (jthread)NULL);

    jvmtiEventCallbacks callbacks = {0};
    callbacks.VMObjectAlloc = &callbackVMObjectAlloc;
    callbacks.ClassLoad = &ClassLoad;

    (*jvmti)->SetEventCallbacks(jvmti, &callbacks, (jint)sizeof(callbacks));

//events end-----------------


    jint foundClasses = 0;
    jclass* classes;

    (*jvmti)->GetLoadedClasses(jvmti, &foundClasses, &classes);

    printf("we found some classes %d\n", foundClasses);

    (*jvmti)->IterateOverHeap(jvmti, JVMTI_HEAP_OBJECT_UNTAGGED, &HeapObject, NULL); //complete heap whether reachable or not; finds ~40k
    //(*jvmti)->IterateOverReachableObjects(jvmti, &HeapRoot, NULL, NULL, NULL); //classloaders, root heap objects etc; finds ~10
    //(*jvmti)->IterateOverReachableObjects(jvmti, NULL, &StackReferences, NULL, NULL); //objs from stack? finds ~10

    //(*jvmti)->IterateOverReachableObjects(jvmti, NULL, NULL, &HeapReferences, NULL); //all reachable objects? finds ~400


    for(size_t i = 0; i < foundClasses; i++)
    {
        jclass class = classes[i];

        char* classname, *generic;
        (*jvmti)->GetClassSignature(jvmti, class, &classname, &generic);

        if(strstr(classname, "java/")  || strstr(classname, "sun/") 
        || strstr(classname, "javax/") || strstr(classname, "jdk/internal/"))
            continue;


        jint fields = 0;
        jfieldID* fieldIds;
        (*jvmti)->GetClassFields(jvmti, class, &fields, &fieldIds);

        
        if(fields < 1)
            continue;

        printf("%s has %d fields \n", classname, fields);

        (*jvmti)->IterateOverInstancesOfClass(jvmti, class, JVMTI_HEAP_OBJECT_TAGGED, &Objectfound, classname);
    }


    printf("were done iterating and we found %d objects, we have %d from known classes \n", objectsFound, objectsFromClass);


    jlong tags[1] = {2345};  
    jint count = 0;
    jobject* objs;
    jlong* tagRes;
    (*jvmti)->GetObjectsWithTags(jvmti, 1, tags, &count, &objs, &tagRes);


    jclass ob = (*jni)->FindClass(jni, "java/lang/Object");
    jobjectArray objects = (*jni)->NewObjectArray(jni, count, ob, NULL);

    for(size_t i = 0; i < count; i++)
        (*jni)->SetObjectArrayElement(jni, objects, i, objs[i]);

    printf("we copied %d objects", count);

    
    
    jclass cls = (*jni)->FindClass(jni, "Agent/Loader");
    if(cls != NULL)
    {
        jmethodID mid = (*jni)->GetStaticMethodID(jni, cls, "loadexplorer", "([Ljava/lang/Object;)V");
        if(mid != NULL)
        {
            (*jni)->CallStaticVoidMethod(jni, cls, mid, objects);
        }

    }
    
    
    return JNI_OK;            
}



JNIEXPORT jint /*JNICALL*/ Agent_OnLoad(JavaVM *jvm, char *options, void *reserved) {
    return Agent_OnAttach(jvm, options, reserved);              
}


JNIEXPORT void /*JNICALL*/ Agent_OnUnload(JavaVM *vm) {
             
}
