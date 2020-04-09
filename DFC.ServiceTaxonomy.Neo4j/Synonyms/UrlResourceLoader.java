/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.NoSuchFileException;
import java.nio.file.Path;
import org.apache.lucene.analysis.util.ResourceLoader;
import org.apache.lucene.analysis.util.ClasspathResourceLoader;
import org.apache.lucene.*;
import java.net.*;

/**
 * Simple {@link ResourceLoader} that opens resource files
 * from the given URL
 */
public final class UrlResourceLoader implements ResourceLoader {
  private final String baseUrl;
  private final ResourceLoader delegate;
  
  public UrlResourceLoader() {
    this(new ClasspathResourceLoader());
  }
  
  public UrlResourceLoader(String baseUrl) {
    this(baseUrl, new ClasspathResourceLoader());
  }

  /**
   * Creates a resource loader that resolves resources against the given
   * base directory (may be {@code null} to refer to CWD).
   * Files not found in file system and class lookups are delegated to context
   * classloader.
   */
  public UrlResourceLoader(String baseUrl, ClassLoader delegate) {
    this(baseUrl, new ClasspathResourceLoader(delegate));
  }

  /**
   * Creates a resource loader that resolves resources against the given
   * base directory (may be {@code null} to refer to CWD).
   * Files not found in file system and class lookups are delegated
   * to the given delegate {@link ResourceLoader}.
   */
  public UrlResourceLoader(String baseUrl, ResourceLoader delegate) {
    this.baseUrl = baseUrl;
    this.delegate = delegate;
  }
  
  public UrlResourceLoader(ResourceLoader delegate) {
    this.delegate = delegate;
	this.baseUrl = "";
  }

  @Override
  public InputStream openResource(String resource) throws IOException {
    try {
		System.out.print("Reading file from: " + resource);  
		InputStream input = new URL(resource).openStream();
      return input;
    } catch (FileNotFoundException | NoSuchFileException fnfe) {
      return delegate.openResource(resource);
    }
  }

  @Override
  public <T> T newInstance(String cname, Class<T> expectedType) {
    return delegate.newInstance(cname, expectedType);
  }

  @Override
  public <T> Class<? extends T> findClass(String cname, Class<T> expectedType) {
    return delegate.findClass(cname, expectedType);
  }
}